using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class SceneTransitionService : ISceneTransitionService
    {
        private readonly ISceneLoadingView _loadingView;
        private readonly ISceneReadinessService _readinessService;
        private readonly ISceneGameplayGate _gameplayGate;
        private readonly IScenePayloadStore _payloadStore;
        private readonly IUnitySceneOperations _sceneOperations;
        private readonly SceneTransitionProgressWeights _weights;
        private readonly SemaphoreSlim _transitionLock = new SemaphoreSlim(1, 1);
        private float _lastProgress;

        public SceneTransitionService(
            ISceneLoadingView loadingView,
            ISceneReadinessService readinessService,
            ISceneGameplayGate gameplayGate,
            IScenePayloadStore payloadStore,
            IUnitySceneOperations sceneOperations,
            SceneTransitionProgressWeights weights = null)
        {
            _loadingView = loadingView ?? throw new ArgumentNullException(nameof(loadingView));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _gameplayGate = gameplayGate ?? throw new ArgumentNullException(nameof(gameplayGate));
            _payloadStore = payloadStore ?? throw new ArgumentNullException(nameof(payloadStore));
            _sceneOperations = sceneOperations ?? throw new ArgumentNullException(nameof(sceneOperations));
            _weights = weights ?? new SceneTransitionProgressWeights();
        }

        public bool IsTransitioning { get; private set; }
        public SceneTransitionStatus CurrentStatus { get; private set; } = SceneTransitionStatus.Idle;

        public async Task<SceneTransitionResult> TransitionAsync(
            SceneTransitionRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!await _transitionLock.WaitAsync(0, cancellationToken))
            {
                return SceneTransitionResult.Failed(
                    request.TargetSceneName,
                    new SceneTransitionError("Transition Busy", "A scene transition is already running."));
            }

            IsTransitioning = true;
            _lastProgress = 0f;
            string previousSceneName = _sceneOperations.ActiveSceneName;
            bool loadingShown = false;

            try
            {
                if (!_sceneOperations.CanLoadScene(request.TargetSceneName))
                {
                    return SceneTransitionResult.Failed(
                        request.TargetSceneName,
                        new SceneTransitionError("Scene Not Found", $"Scene '{request.TargetSceneName}' is not enabled in Build Settings."));
                }

                _gameplayGate.Block();
                if (request.Payload != null)
                    _payloadStore.Store(request.Payload);
                else
                    _payloadStore.Clear();

                float startedAt = Time.realtimeSinceStartup;

                await SetStatusAsync(SceneTransitionStatus.OpeningLoadingScreen, 0f, "Preparando...", cancellationToken);
                await _loadingView.ShowAsync(cancellationToken);
                loadingShown = true;
                SetProgress(_weights.OpeningEnd);

                cancellationToken.ThrowIfCancellationRequested();
                await SetStatusAsync(SceneTransitionStatus.Preparing, _weights.OpeningEnd, "Preparando...", cancellationToken);

                ISceneLoadOperation operation = _sceneOperations.LoadSceneAsync(request.TargetSceneName, LoadSceneMode.Additive);
                if (operation == null)
                    throw new InvalidOperationException($"Unity did not create a load operation for scene '{request.TargetSceneName}'.");

                operation.AllowSceneActivation = false;
                await SetStatusAsync(SceneTransitionStatus.LoadingScene, _weights.OpeningEnd, "Carregando cenário...", cancellationToken);

                while (operation.Progress < 0.9f)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    float normalizedLoadProgress = Mathf.Clamp01(operation.Progress / 0.9f);
                    SetProgress(Mathf.Lerp(_weights.OpeningEnd, _weights.LoadingEnd, normalizedLoadProgress));
                    await Task.Yield();
                }

                await SetStatusAsync(SceneTransitionStatus.WaitingForActivation, _weights.LoadingEnd, "Ativando cena...", cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                CurrentStatus = SceneTransitionStatus.ActivatingScene;
                _loadingView.SetStatus("Ativando cena...");
                operation.AllowSceneActivation = true;
                while (!operation.IsDone)
                {
                    SetProgress(_weights.ActivationEnd);
                    await Task.Yield();
                }

                await SetStatusAsync(SceneTransitionStatus.ResolvingSceneScope, _weights.ActivationEnd, "Inicializando sistemas...", cancellationToken);
                Scene loadedScene = _sceneOperations.GetScene(request.TargetSceneName);
                var initializationContext = new SceneInitializationContext(
                    loadedScene,
                    _payloadStore.Peek(),
                    request);
                var warmupContext = new SceneWarmupContext(
                    loadedScene,
                    _payloadStore.Peek(),
                    request);

                using (CancellationTokenSource initializationTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    initializationTimeout.CancelAfter(TimeSpan.FromSeconds(Mathf.Max(1f, request.InitializationTimeoutSeconds)));

                    try
                    {
                        await SetStatusAsync(SceneTransitionStatus.InitializingScene, _weights.ActivationEnd, "Inicializando sistemas...", initializationTimeout.Token);
                        SceneReadinessResult initializationResult = await _readinessService.InitializeAsync(
                            initializationContext,
                            new Progress<float>(value => SetProgress(Mathf.Lerp(_weights.ActivationEnd, _weights.InitializationEnd, value))),
                            initializationTimeout.Token);
                        EnsureReady(initializationResult, "Scene initialization failed.");

                        await SetStatusAsync(SceneTransitionStatus.WarmingUpScene, _weights.InitializationEnd, "Preparando o mundo...", initializationTimeout.Token);
                        SceneReadinessResult warmupResult = await _readinessService.WarmUpAsync(
                            warmupContext,
                            new Progress<float>(value => SetProgress(Mathf.Lerp(_weights.InitializationEnd, _weights.WarmupEnd, value))),
                            initializationTimeout.Token);
                        EnsureReady(warmupResult, "Scene warmup failed.");

                        _payloadStore.Consume();

                        await _sceneOperations.WaitForFramesAsync(
                            request.StabilizationFrameCount,
                            includeEndOfFrame: true,
                            cancellationToken: initializationTimeout.Token);

                        SceneReadinessResult finalReadiness = await _readinessService.ValidateAsync(
                            initializationContext,
                            initializationTimeout.Token);
                        EnsureReady(finalReadiness, "Scene readiness validation failed.");
                    }
                    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                    {
                        throw new TimeoutException(
                            $"Scene initialization timed out after {request.InitializationTimeoutSeconds:0.##} seconds.");
                    }
                }

                if (request.SetAsActiveScene)
                {
                    await SetStatusAsync(SceneTransitionStatus.SwitchingActiveScene, _weights.WarmupEnd, "Finalizando...", cancellationToken);
                    _sceneOperations.SetActiveScene(request.TargetSceneName);
                }

                if (request.Mode == SceneTransitionMode.ReplaceCurrent &&
                    request.UnloadPreviousScene &&
                    !string.IsNullOrWhiteSpace(previousSceneName) &&
                    previousSceneName != request.TargetSceneName)
                {
                    await SetStatusAsync(SceneTransitionStatus.UnloadingPreviousScene, 0.99f, "Finalizando...", cancellationToken);
                    await _sceneOperations.UnloadSceneAsync(previousSceneName);
                }

                await WaitForMinimumDurationAsync(startedAt, request.MinimumLoadingScreenDuration, cancellationToken);
                SetProgress(1f);
                CurrentStatus = SceneTransitionStatus.ClosingLoadingScreen;
                await _loadingView.HideAsync(cancellationToken);
                loadingShown = false;

                CurrentStatus = SceneTransitionStatus.Completed;
                return SceneTransitionResult.Completed(request.TargetSceneName);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = SceneTransitionStatus.Cancelled;
                if (loadingShown)
                    await HideWithoutThrowAsync();
                return SceneTransitionResult.Cancelled(request.TargetSceneName);
            }
            catch (Exception exception)
            {
                CurrentStatus = SceneTransitionStatus.Failed;
                var error = new SceneTransitionError("Loading Failed", "Não foi possível carregar a cena.", exception);
                _loadingView.ShowError(error);
                Debug.LogException(exception);
                return SceneTransitionResult.Failed(request.TargetSceneName, error);
            }
            finally
            {
                _payloadStore.Clear();
                _gameplayGate.Release();
                IsTransitioning = false;
                if (CurrentStatus != SceneTransitionStatus.Completed &&
                    CurrentStatus != SceneTransitionStatus.Failed &&
                    CurrentStatus != SceneTransitionStatus.Cancelled)
                    CurrentStatus = SceneTransitionStatus.Idle;
                _transitionLock.Release();
            }
        }

        private async Task SetStatusAsync(
            SceneTransitionStatus status,
            float progress,
            string statusText,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CurrentStatus = status;
            _loadingView.SetStatus(statusText);
            SetProgress(progress);
            await Task.Yield();
        }

        private void SetProgress(float normalizedProgress)
        {
            _lastProgress = Mathf.Clamp01(Mathf.Max(_lastProgress, normalizedProgress));
            _loadingView.SetProgress(_lastProgress);
        }

        private static void EnsureReady(SceneReadinessResult result, string fallbackMessage)
        {
            if (result != null && result.IsReady)
                return;

            if (result != null && result.Errors.Count > 0)
                throw new InvalidOperationException(result.Errors[0].Message, result.Errors[0].Exception);

            if (result != null && result.PendingSystems.Count > 0)
                throw new InvalidOperationException($"{fallbackMessage} Pending: {string.Join(", ", result.PendingSystems)}");

            throw new InvalidOperationException(fallbackMessage);
        }

        private static async Task WaitForMinimumDurationAsync(
            float startedAt,
            float minimumDuration,
            CancellationToken cancellationToken)
        {
            if (minimumDuration <= 0f)
                return;

            while (Time.realtimeSinceStartup - startedAt < minimumDuration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        private async Task HideWithoutThrowAsync()
        {
            try
            {
                await _loadingView.HideAsync(CancellationToken.None);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
