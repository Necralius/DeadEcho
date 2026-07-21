using System;
using System.Linq;
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
        private SceneTransitionRequest _lastFailedRetryRequest;
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
                    new SceneTransitionError(
                        "Transição em andamento",
                        "Aguarde o carregamento atual terminar.",
                        code: SceneTransitionErrorCode.Unknown,
                        policy: SceneTransitionErrorPolicy.Recoverable));
            }

            string transitionId = Guid.NewGuid().ToString("N");
            string previousSceneName = _sceneOperations.ActiveSceneName;
            bool loadingShown = false;
            bool targetActivated = false;
            bool previousUnloaded = false;
            _lastProgress = 0f;
            IsTransitioning = true;

            float transitionStartedAt = Time.realtimeSinceStartup;
            float loadingShownAt = 0f;
            var diagnostics = new SceneTransitionDiagnostics
            {
                TransitionId = transitionId,
                PreviousSceneName = previousSceneName,
                TargetSceneName = request.TargetSceneName
            };

            try
            {
                _gameplayGate.Block();
                StorePayload(request);

                Debug.Log($"[SceneTransition:{transitionId}] Start previous='{previousSceneName}' target='{request.TargetSceneName}' status={CurrentStatus}.");

                await SetStatusAsync(SceneTransitionStatus.OpeningLoadingScreen, 0f, "Preparando...", cancellationToken);
                await _loadingView.ShowAsync(cancellationToken);
                loadingShown = true;
                loadingShownAt = Time.realtimeSinceStartup;
                SetProgress(_weights.OpeningEnd);

                if (!_sceneOperations.CanLoadScene(request.TargetSceneName))
                {
                    throw new SceneTransitionException(
                        SceneTransitionErrorCode.SceneNotFound,
                        SceneTransitionErrorPolicy.Fatal,
                        $"Scene '{request.TargetSceneName}' is not enabled in Build Settings.");
                }

                cancellationToken.ThrowIfCancellationRequested();
                await SetStatusAsync(SceneTransitionStatus.Preparing, _weights.OpeningEnd, "Preparando...", cancellationToken);

                float sceneLoadStartedAt = Time.realtimeSinceStartup;
                ISceneLoadOperation operation = _sceneOperations.LoadSceneAsync(request.TargetSceneName, LoadSceneMode.Additive);
                if (operation == null)
                {
                    throw new SceneTransitionException(
                        SceneTransitionErrorCode.LoadingStartFailed,
                        SceneTransitionErrorPolicy.Retryable,
                        $"Unity did not create a load operation for scene '{request.TargetSceneName}'.",
                        canRetry: true);
                }

                operation.AllowSceneActivation = false;
                await SetStatusAsync(SceneTransitionStatus.LoadingScene, _weights.OpeningEnd, "Carregando cenário...", cancellationToken);

                while (operation.Progress < 0.9f)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    float normalizedLoadProgress = Mathf.Clamp01(operation.Progress / 0.9f);
                    SetProgress(Mathf.Lerp(_weights.OpeningEnd, _weights.LoadingEnd, normalizedLoadProgress));
                    await Task.Yield();
                }

                diagnostics.SceneLoadDuration = Time.realtimeSinceStartup - sceneLoadStartedAt;
                await SetStatusAsync(SceneTransitionStatus.WaitingForActivation, _weights.LoadingEnd, "Ativando cena...", cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                float activationStartedAt = Time.realtimeSinceStartup;
                CurrentStatus = SceneTransitionStatus.ActivatingScene;
                _loadingView.SetStatus("Ativando cena...");
                operation.AllowSceneActivation = true;
                while (!operation.IsDone)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    SetProgress(_weights.ActivationEnd);
                    await Task.Yield();
                }

                targetActivated = true;
                diagnostics.ActivationDuration = Time.realtimeSinceStartup - activationStartedAt;

                await SetStatusAsync(SceneTransitionStatus.ResolvingSceneScope, _weights.ActivationEnd, "Inicializando sistemas...", cancellationToken);
                Scene loadedScene = _sceneOperations.GetScene(request.TargetSceneName);
                var initializationContext = new SceneInitializationContext(loadedScene, _payloadStore.Peek(), request);
                var warmupContext = new SceneWarmupContext(loadedScene, _payloadStore.Peek(), request);

                using (CancellationTokenSource initializationTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    initializationTimeout.CancelAfter(TimeSpan.FromSeconds(Mathf.Max(1f, request.InitializationTimeoutSeconds)));

                    try
                    {
                        float initializationStartedAt = Time.realtimeSinceStartup;
                        await SetStatusAsync(SceneTransitionStatus.InitializingScene, _weights.ActivationEnd, "Inicializando sistemas...", initializationTimeout.Token);
                        SceneReadinessResult initializationResult = await _readinessService.InitializeAsync(
                            initializationContext,
                            new Progress<float>(value => SetProgress(Mathf.Lerp(_weights.ActivationEnd, _weights.InitializationEnd, value))),
                            initializationTimeout.Token);
                        EnsureReady(initializationResult, "Scene initialization failed.");
                        diagnostics.InitializationDuration = Time.realtimeSinceStartup - initializationStartedAt;

                        float warmupStartedAt = Time.realtimeSinceStartup;
                        await SetStatusAsync(SceneTransitionStatus.WarmingUpScene, _weights.InitializationEnd, "Preparando o mundo...", initializationTimeout.Token);
                        SceneReadinessResult warmupResult = await _readinessService.WarmUpAsync(
                            warmupContext,
                            new Progress<float>(value => SetProgress(Mathf.Lerp(_weights.InitializationEnd, _weights.WarmupEnd, value))),
                            initializationTimeout.Token);
                        EnsureReady(warmupResult, "Scene warmup failed.");
                        diagnostics.WarmupDuration = Time.realtimeSinceStartup - warmupStartedAt;

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
                        throw new SceneTransitionException(
                            SceneTransitionErrorCode.Timeout,
                            SceneTransitionErrorPolicy.Retryable,
                            $"Scene initialization timed out after {request.InitializationTimeoutSeconds:0.##} seconds.",
                            canRetry: true);
                    }
                }

                if (request.SetAsActiveScene)
                {
                    await SetStatusAsync(SceneTransitionStatus.SwitchingActiveScene, _weights.WarmupEnd, "Finalizando...", cancellationToken);
                    if (!_sceneOperations.SetActiveScene(request.TargetSceneName))
                    {
                        throw new SceneTransitionException(
                            SceneTransitionErrorCode.SetActiveSceneFailed,
                            SceneTransitionErrorPolicy.Recoverable,
                            $"Unity failed to set scene '{request.TargetSceneName}' as active.");
                    }
                }

                if (request.Mode == SceneTransitionMode.ReplaceCurrent &&
                    request.UnloadPreviousScene &&
                    !string.IsNullOrWhiteSpace(previousSceneName) &&
                    previousSceneName != request.TargetSceneName)
                {
                    await SetStatusAsync(SceneTransitionStatus.UnloadingPreviousScene, 0.99f, "Finalizando...", cancellationToken);
                    float unloadStartedAt = Time.realtimeSinceStartup;
                    try
                    {
                        await _sceneOperations.UnloadSceneAsync(previousSceneName);
                        previousUnloaded = true;
                        diagnostics.UnloadDuration = Time.realtimeSinceStartup - unloadStartedAt;
                    }
                    catch (Exception exception)
                    {
                        throw new SceneTransitionException(
                            SceneTransitionErrorCode.UnloadFailed,
                            SceneTransitionErrorPolicy.Recoverable,
                            $"Unity failed to unload previous scene '{previousSceneName}'.",
                            exception);
                    }
                }

                await WaitForMinimumDurationAsync(transitionStartedAt, request.MinimumLoadingScreenDuration, cancellationToken);
                SetProgress(1f);
                CurrentStatus = SceneTransitionStatus.ClosingLoadingScreen;
                await _loadingView.HideAsync(cancellationToken);
                loadingShown = false;

                CurrentStatus = SceneTransitionStatus.Completed;
                diagnostics.FinalStatus = CurrentStatus;
                diagnostics.LoadingScreenOpenDuration = Time.realtimeSinceStartup - loadingShownAt;
                diagnostics.TotalTransitionDuration = Time.realtimeSinceStartup - transitionStartedAt;
                Debug.Log(BuildDiagnosticsLog(diagnostics));
                return SceneTransitionResult.Completed(request.TargetSceneName, diagnostics);
            }
            catch (OperationCanceledException)
            {
                CurrentStatus = SceneTransitionStatus.Cancelled;
                await CleanupFailedTargetAsync(request, previousSceneName, targetActivated, previousUnloaded);
                SceneTransitionResult result = SceneTransitionResult.Cancelled(request.TargetSceneName, CompleteDiagnostics(diagnostics, SceneTransitionStatus.Cancelled, loadingShown, loadingShownAt, transitionStartedAt));
                if (loadingShown)
                    _loadingView.ShowError(result.Error, () => RetryAsync(request), () => ReturnToMainMenuAsync(request));
                Debug.Log(BuildDiagnosticsLog(result.Diagnostics));
                return result;
            }
            catch (Exception exception)
            {
                CurrentStatus = SceneTransitionStatus.Failed;
                await CleanupFailedTargetAsync(request, previousSceneName, targetActivated, previousUnloaded);
                SceneTransitionError error = CreateError(exception, !previousUnloaded);
                _lastFailedRetryRequest = error.CanRetry ? CloneRequest(request) : null;
                diagnostics = CompleteDiagnostics(diagnostics, SceneTransitionStatus.Failed, loadingShown, loadingShownAt, transitionStartedAt);
                if (loadingShown)
                    _loadingView.ShowError(error, error.CanRetry ? (() => RetryAsync(request)) : null, () => ReturnToMainMenuAsync(request));
                Debug.LogException(exception);
                Debug.Log(BuildDiagnosticsLog(diagnostics));
                return SceneTransitionResult.Failed(request.TargetSceneName, error, diagnostics);
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

        private void StorePayload(SceneTransitionRequest request)
        {
            if (request.Payload != null)
                _payloadStore.Store(request.Payload);
            else
                _payloadStore.Clear();
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
            {
                SceneTransitionErrorCode code = ClassifyReadinessError(result.Errors[0]);
                throw new SceneTransitionException(
                    code,
                    code == SceneTransitionErrorCode.CorruptedSave || code == SceneTransitionErrorCode.InvalidSave
                        ? SceneTransitionErrorPolicy.Recoverable
                        : SceneTransitionErrorPolicy.Retryable,
                    result.Errors[0].Message,
                    result.Errors[0].Exception,
                    code != SceneTransitionErrorCode.CorruptedSave && code != SceneTransitionErrorCode.InvalidSave);
            }

            if (result != null && result.PendingSystems.Count > 0)
            {
                SceneTransitionErrorCode code = result.PendingSystems.Contains("SceneLifetimeScope")
                    ? SceneTransitionErrorCode.LifetimeScopeMissing
                    : SceneTransitionErrorCode.CriticalInitializerMissing;
                throw new SceneTransitionException(
                    code,
                    SceneTransitionErrorPolicy.Fatal,
                    $"{fallbackMessage} Pending: {string.Join(", ", result.PendingSystems)}");
            }

            throw new SceneTransitionException(
                SceneTransitionErrorCode.InitializationFailed,
                SceneTransitionErrorPolicy.Retryable,
                fallbackMessage,
                canRetry: true);
        }

        private async Task RetryAsync(SceneTransitionRequest request)
        {
            SceneTransitionRequest retryRequest = _lastFailedRetryRequest ?? CloneRequest(request);
            _lastFailedRetryRequest = null;
            await TransitionAsync(retryRequest, CancellationToken.None);
        }

        private async Task ReturnToMainMenuAsync(SceneTransitionRequest failedRequest)
        {
            _payloadStore.Clear();
            if (string.IsNullOrWhiteSpace(failedRequest.MainMenuSceneName) ||
                failedRequest.TargetSceneName == failedRequest.MainMenuSceneName)
            {
                await HideWithoutThrowAsync();
                return;
            }

            await TransitionAsync(
                new SceneTransitionRequest(failedRequest.MainMenuSceneName)
                {
                    Payload = new ScenePayload("ReturnToMainMenu"),
                    MinimumLoadingScreenDuration = 0f,
                    RequireSceneLifetimeScope = false
                },
                CancellationToken.None);
        }

        private async Task CleanupFailedTargetAsync(
            SceneTransitionRequest request,
            string previousSceneName,
            bool targetActivated,
            bool previousUnloaded)
        {
            if (!targetActivated || previousUnloaded || request.TargetSceneName == previousSceneName)
                return;

            try
            {
                await _sceneOperations.UnloadSceneAsync(request.TargetSceneName);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static SceneTransitionError CreateError(Exception exception, bool canRetrySafely)
        {
            if (exception is SceneTransitionException sceneException)
            {
                return new SceneTransitionError(
                    PlayerTitle(sceneException.Code),
                    PlayerMessage(sceneException.Code),
                    sceneException,
                    sceneException.Code,
                    sceneException.Policy,
                    sceneException.CanRetry && canRetrySafely,
                    sceneException.Policy != SceneTransitionErrorPolicy.Fatal);
            }

            return new SceneTransitionError(
                "Falha no carregamento",
                "Não foi possível carregar a cena. Volte ao menu e tente novamente.",
                exception,
                SceneTransitionErrorCode.Unknown,
                SceneTransitionErrorPolicy.Retryable,
                canRetrySafely,
                true);
        }

        private static SceneTransitionRequest CloneRequest(SceneTransitionRequest request)
        {
            return new SceneTransitionRequest(request.TargetSceneName)
            {
                Mode = request.Mode,
                SetAsActiveScene = request.SetAsActiveScene,
                UnloadPreviousScene = request.UnloadPreviousScene,
                MinimumLoadingScreenDuration = request.MinimumLoadingScreenDuration,
                InitializationTimeoutSeconds = request.InitializationTimeoutSeconds,
                StabilizationFrameCount = request.StabilizationFrameCount,
                RequireSceneLifetimeScope = request.RequireSceneLifetimeScope,
                MainMenuSceneName = request.MainMenuSceneName,
                Payload = request.Payload
            };
        }

        private static SceneTransitionErrorCode ClassifyReadinessError(SceneInitializationError error)
        {
            string text = $"{error.SystemName} {error.Message}".ToLowerInvariant();
            if (text.Contains("corrupt"))
                return SceneTransitionErrorCode.CorruptedSave;
            if (text.Contains("save") && (text.Contains("invalid") || text.Contains("invalido") || text.Contains("inválido")))
                return SceneTransitionErrorCode.InvalidSave;
            if (text.Contains("addressable") || text.Contains("asset"))
                return SceneTransitionErrorCode.AddressablesFailed;

            return SceneTransitionErrorCode.InitializationFailed;
        }

        private static string PlayerTitle(SceneTransitionErrorCode code)
        {
            switch (code)
            {
                case SceneTransitionErrorCode.SceneNotFound:
                case SceneTransitionErrorCode.LifetimeScopeMissing:
                case SceneTransitionErrorCode.CriticalInitializerMissing:
                    return "Erro de configuração";
                case SceneTransitionErrorCode.Timeout:
                    return "Carregamento demorou demais";
                case SceneTransitionErrorCode.InvalidSave:
                case SceneTransitionErrorCode.CorruptedSave:
                    return "Save indisponível";
                default:
                    return "Falha no carregamento";
            }
        }

        private static string PlayerMessage(SceneTransitionErrorCode code)
        {
            switch (code)
            {
                case SceneTransitionErrorCode.SceneNotFound:
                    return "A cena solicitada não está disponível nesta versão.";
                case SceneTransitionErrorCode.LifetimeScopeMissing:
                    return "A cena carregada não possui os serviços necessários.";
                case SceneTransitionErrorCode.CriticalInitializerMissing:
                    return "A cena carregada não possui inicializadores necessários.";
                case SceneTransitionErrorCode.Timeout:
                    return "A cena não ficou pronta a tempo. Você pode tentar novamente.";
                case SceneTransitionErrorCode.InvalidSave:
                    return "O save selecionado não pode ser carregado.";
                case SceneTransitionErrorCode.CorruptedSave:
                    return "O save selecionado parece estar corrompido.";
                case SceneTransitionErrorCode.UnloadFailed:
                    return "A nova cena carregou, mas houve falha ao limpar a cena anterior.";
                case SceneTransitionErrorCode.SetActiveSceneFailed:
                    return "A cena carregou, mas não pôde ser ativada.";
                default:
                    return "Não foi possível carregar a cena. Volte ao menu e tente novamente.";
            }
        }

        private static SceneTransitionDiagnostics CompleteDiagnostics(
            SceneTransitionDiagnostics diagnostics,
            SceneTransitionStatus status,
            bool loadingShown,
            float loadingShownAt,
            float transitionStartedAt)
        {
            diagnostics.FinalStatus = status;
            diagnostics.LoadingScreenOpenDuration = loadingShown ? Time.realtimeSinceStartup - loadingShownAt : 0f;
            diagnostics.TotalTransitionDuration = Time.realtimeSinceStartup - transitionStartedAt;
            return diagnostics;
        }

        private static string BuildDiagnosticsLog(SceneTransitionDiagnostics diagnostics)
        {
            return
                $"[SceneTransition:{diagnostics.TransitionId}] Result={diagnostics.FinalStatus} " +
                $"previous='{diagnostics.PreviousSceneName}' target='{diagnostics.TargetSceneName}' " +
                $"load={diagnostics.SceneLoadDuration:0.000}s activation={diagnostics.ActivationDuration:0.000}s " +
                $"initialization={diagnostics.InitializationDuration:0.000}s warmup={diagnostics.WarmupDuration:0.000}s " +
                $"unload={diagnostics.UnloadDuration:0.000}s loadingOpen={diagnostics.LoadingScreenOpenDuration:0.000}s " +
                $"total={diagnostics.TotalTransitionDuration:0.000}s";
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

        private sealed class SceneTransitionException : Exception
        {
            public SceneTransitionException(
                SceneTransitionErrorCode code,
                SceneTransitionErrorPolicy policy,
                string message,
                Exception innerException = null,
                bool canRetry = false)
                : base(message, innerException)
            {
                Code = code;
                Policy = policy;
                CanRetry = canRetry;
            }

            public SceneTransitionErrorCode Code { get; }
            public SceneTransitionErrorPolicy Policy { get; }
            public bool CanRetry { get; }
        }
    }
}
