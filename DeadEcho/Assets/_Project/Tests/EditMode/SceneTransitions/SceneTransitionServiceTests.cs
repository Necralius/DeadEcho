using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Project.Core.Services;
using Project.Infrastructure.SceneTransitions;
using Project.UI.Runtime;
using Project.UI.SceneTransitions;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace Project.Tests.EditMode.SceneTransitions
{
    public sealed class SceneTransitionServiceTests
    {
        [Test]
        public async Task FailsForSceneNotInBuildSettings()
        {
            FakeSceneOperations scenes = new FakeSceneOperations(false);
            FakeLoadingView loading = new FakeLoadingView();
            SceneGameplayGate gate = new SceneGameplayGate();
            ScenePayloadStore payload = new ScenePayloadStore();
            SceneTransitionService service = CreateService(loading, gate, payload, scenes);

            SceneTransitionResult result = await service.TransitionAsync(new SceneTransitionRequest("MissingScene"));

            Assert.AreEqual(SceneTransitionStatus.Failed, result.Status);
            Assert.IsFalse(service.IsTransitioning);
            Assert.IsFalse(gate.IsGameplayBlocked);
            Assert.AreEqual(0, loading.ShowCount);
        }

        [Test]
        public async Task RejectsSecondTransitionWhileFirstIsRunning()
        {
            FakeSceneOperations scenes = new FakeSceneOperations(true) { Operation = new FakeLoadOperation { ProgressValue = 0f } };
            FakeLoadingView loading = new FakeLoadingView();
            SceneTransitionService service = CreateService(loading, new SceneGameplayGate(), new ScenePayloadStore(), scenes);
            using CancellationTokenSource cancellation = new CancellationTokenSource();

            Task<SceneTransitionResult> first = service.TransitionAsync(
                new SceneTransitionRequest("Gameplay") { MinimumLoadingScreenDuration = 0f },
                cancellation.Token);

            while (!service.IsTransitioning)
                await Task.Yield();

            SceneTransitionResult second = await service.TransitionAsync(new SceneTransitionRequest("Gameplay"));
            cancellation.Cancel();
            await first;

            Assert.AreEqual(SceneTransitionStatus.Failed, second.Status);
            Assert.IsFalse(service.IsTransitioning);
        }

        [Test]
        public async Task BlocksAndReleasesGameplayOnSuccess()
        {
            SceneGameplayGate gate = new SceneGameplayGate();
            SceneTransitionService service = CreateService(new FakeLoadingView(), gate, new ScenePayloadStore(), new FakeSceneOperations(true));

            SceneTransitionResult result = await service.TransitionAsync(
                new SceneTransitionRequest("Gameplay") { MinimumLoadingScreenDuration = 0f });

            Assert.IsTrue(result.Succeeded);
            Assert.IsFalse(gate.IsGameplayBlocked);
        }

        [Test]
        public async Task ReleasesGameplayAndClearsPayloadAfterFailure()
        {
            SceneGameplayGate gate = new SceneGameplayGate();
            ScenePayloadStore payload = new ScenePayloadStore();
            FakeSceneOperations scenes = new FakeSceneOperations(true) { ThrowOnUnload = true };
            SceneTransitionService service = CreateService(new FakeLoadingView(), gate, payload, scenes);

            SceneTransitionResult result = await service.TransitionAsync(
                new SceneTransitionRequest("Gameplay")
                {
                    Payload = new ScenePayload("NewGame"),
                    MinimumLoadingScreenDuration = 0f
                });

            Assert.AreEqual(SceneTransitionStatus.Failed, result.Status);
            Assert.IsFalse(gate.IsGameplayBlocked);
            Assert.IsFalse(payload.HasPayload);
        }

        [Test]
        public async Task DoesNotUnloadPreviousSceneBeforeTargetIsInitialized()
        {
            FakeSceneOperations scenes = new FakeSceneOperations(true);
            RecordingReadiness readiness = new RecordingReadiness(scenes.Events);
            SceneTransitionService service = CreateService(
                new FakeLoadingView(),
                new SceneGameplayGate(),
                new ScenePayloadStore(),
                scenes,
                readiness);

            await service.TransitionAsync(new SceneTransitionRequest("Gameplay") { MinimumLoadingScreenDuration = 0f });

            CollectionAssert.AreEqual(
                new[] { "load", "activate", "initialize", "warmup", "wait-frames:2:True", "validate", "set-active", "unload:Menu" },
                scenes.Events);
        }

        [Test]
        public async Task KeepsGameplayBlockedDuringInitialization()
        {
            SceneGameplayGate gate = new SceneGameplayGate();
            FakeSceneOperations scenes = new FakeSceneOperations(true);
            GateCheckingReadiness readiness = new GateCheckingReadiness(gate);
            SceneTransitionService service = CreateService(
                new FakeLoadingView(),
                gate,
                new ScenePayloadStore(),
                scenes,
                readiness);

            SceneTransitionResult result = await service.TransitionAsync(
                new SceneTransitionRequest("Gameplay") { MinimumLoadingScreenDuration = 0f });

            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(readiness.WasBlockedDuringInitialization);
            Assert.IsFalse(gate.IsGameplayBlocked);
        }

        [Test]
        public async Task PassesAndClearsPayloadAfterReadiness()
        {
            ScenePayloadStore payload = new ScenePayloadStore();
            FakeSceneOperations scenes = new FakeSceneOperations(true);
            PayloadRecordingReadiness readiness = new PayloadRecordingReadiness();
            SceneTransitionService service = CreateService(
                new FakeLoadingView(),
                new SceneGameplayGate(),
                payload,
                scenes,
                readiness);

            await service.TransitionAsync(
                new SceneTransitionRequest("Gameplay")
                {
                    Payload = new ScenePayload("LoadGame", new Dictionary<string, string> { { "saveId", "slot-01" } }),
                    MinimumLoadingScreenDuration = 0f
                });

            Assert.AreEqual("LoadGame", readiness.InitializationPayload.EntryKind);
            Assert.IsTrue(readiness.InitializationPayload.TryGetValue("saveId", out string saveId));
            Assert.AreEqual("slot-01", saveId);
            Assert.IsFalse(payload.HasPayload);
        }

        [Test]
        public async Task FailsAndPreservesPreviousSceneWhenReadinessTimesOut()
        {
            FakeSceneOperations scenes = new FakeSceneOperations(true);
            SceneTransitionService service = CreateService(
                new FakeLoadingView(),
                new SceneGameplayGate(),
                new ScenePayloadStore(),
                scenes,
                new NeverReadyReadiness());

            SceneTransitionResult result = await service.TransitionAsync(
                new SceneTransitionRequest("Gameplay")
                {
                    MinimumLoadingScreenDuration = 0f,
                    InitializationTimeoutSeconds = 1f
                });

            Assert.AreEqual(SceneTransitionStatus.Failed, result.Status);
            CollectionAssert.DoesNotContain(scenes.Events, "unload:Menu");
        }

        [Test]
        public async Task LoadingViewShowsHidesAndPreventsProgressRegression()
        {
            HeadlessUiRoot root = new HeadlessUiRoot();
            SceneLoadingView view = new SceneLoadingView(root);

            await view.ShowAsync();
            view.SetProgress(0.75f);
            view.SetProgress(0.25f);

            LabelText(root.LoadingLayer, "loading-progress-label", "75%");
            Assert.AreEqual(UnityEngine.UIElements.DisplayStyle.Flex, root.LoadingLayer.style.display.value);

            await view.HideAsync();
            Assert.AreEqual(UnityEngine.UIElements.DisplayStyle.None, root.LoadingLayer.style.display.value);
        }

        private static SceneTransitionService CreateService(
            ISceneLoadingView loading,
            SceneGameplayGate gate,
            ScenePayloadStore payload,
            FakeSceneOperations scenes,
            ISceneReadinessService readiness = null)
        {
            return new SceneTransitionService(
                loading,
                readiness ?? new RecordingReadiness(scenes.Events),
                gate,
                payload,
                scenes,
                new SceneTransitionProgressWeights());
        }

        private static void LabelText(UnityEngine.UIElements.VisualElement root, string name, string expected)
        {
            Assert.AreEqual(expected, root.Q<UnityEngine.UIElements.Label>(name).text);
        }

        private sealed class FakeLoadingView : ISceneLoadingView
        {
            public int ShowCount { get; private set; }
            public int HideCount { get; private set; }
            public readonly List<float> Progress = new List<float>();

            public Task ShowAsync(CancellationToken cancellationToken = default)
            {
                ShowCount++;
                return Task.CompletedTask;
            }

            public void SetProgress(float normalizedProgress)
            {
                Progress.Add(normalizedProgress);
            }

            public void SetStatus(string status) { }
            public void SetTip(string tip) { }
            public void ShowError(SceneTransitionError error) { }

            public Task HideAsync(CancellationToken cancellationToken = default)
            {
                HideCount++;
                return Task.CompletedTask;
            }
        }

        private sealed class FakeSceneOperations : IUnitySceneOperations
        {
            private readonly bool _canLoad;

            public FakeSceneOperations(bool canLoad)
            {
                _canLoad = canLoad;
            }

            public string ActiveSceneName { get; set; } = "Menu";
            public FakeLoadOperation Operation { get; set; } = new FakeLoadOperation { ProgressValue = 0.9f };
            public bool ThrowOnUnload { get; set; }
            public List<string> Events { get; } = new List<string>();

            public bool CanLoadScene(string sceneName) => _canLoad;

            public Scene GetScene(string sceneName) => SceneManager.GetActiveScene();

            public ISceneLoadOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
            {
                Events.Add("load");
                return Operation;
            }

            public bool SetActiveScene(string sceneName)
            {
                Events.Add("set-active");
                ActiveSceneName = sceneName;
                return true;
            }

            public Task UnloadSceneAsync(string sceneName)
            {
                Events.Add($"unload:{sceneName}");
                if (ThrowOnUnload)
                    throw new System.InvalidOperationException("Unload failed.");
                return Task.CompletedTask;
            }

            public Task WaitForFramesAsync(int frameCount, bool includeEndOfFrame, CancellationToken cancellationToken = default)
            {
                Events.Add($"wait-frames:{frameCount}:{includeEndOfFrame}");
                return Task.CompletedTask;
            }
        }

        private sealed class FakeLoadOperation : ISceneLoadOperation
        {
            private bool _allowSceneActivation;

            public float ProgressValue { get; set; }
            public float Progress => ProgressValue;
            public bool IsDone { get; private set; }

            public bool AllowSceneActivation
            {
                get => _allowSceneActivation;
                set
                {
                    _allowSceneActivation = value;
                    if (value)
                        IsDone = true;
                }
            }
        }

        private sealed class RecordingReadiness : ISceneReadinessService
        {
            private readonly List<string> _events;

            public RecordingReadiness(List<string> events)
            {
                _events = events;
            }

            public Task<SceneReadinessResult> InitializeAsync(
                SceneInitializationContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                _events.Add("initialize");
                progress?.Report(1f);
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> WarmUpAsync(
                SceneWarmupContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                _events.Add("warmup");
                progress?.Report(1f);
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> ValidateAsync(
                SceneInitializationContext context,
                CancellationToken cancellationToken = default)
            {
                _events.Add("validate");
                return Task.FromResult(SceneReadinessResult.Ready());
            }
        }

        private sealed class GateCheckingReadiness : ISceneReadinessService
        {
            private readonly SceneGameplayGate _gate;

            public GateCheckingReadiness(SceneGameplayGate gate)
            {
                _gate = gate;
            }

            public bool WasBlockedDuringInitialization { get; private set; }

            public Task<SceneReadinessResult> InitializeAsync(
                SceneInitializationContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                WasBlockedDuringInitialization = _gate.IsGameplayBlocked;
                progress?.Report(1f);
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> WarmUpAsync(
                SceneWarmupContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                progress?.Report(1f);
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> ValidateAsync(
                SceneInitializationContext context,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(SceneReadinessResult.Ready());
            }
        }

        private sealed class PayloadRecordingReadiness : ISceneReadinessService
        {
            public ScenePayload InitializationPayload { get; private set; }

            public Task<SceneReadinessResult> InitializeAsync(
                SceneInitializationContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                InitializationPayload = context.Payload;
                progress?.Report(1f);
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> WarmUpAsync(
                SceneWarmupContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                progress?.Report(1f);
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> ValidateAsync(
                SceneInitializationContext context,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(SceneReadinessResult.Ready());
            }
        }

        private sealed class NeverReadyReadiness : ISceneReadinessService
        {
            public async Task<SceneReadinessResult> InitializeAsync(
                SceneInitializationContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                await Task.Delay(System.Threading.Timeout.InfiniteTimeSpan, cancellationToken);
                return SceneReadinessResult.Ready();
            }

            public Task<SceneReadinessResult> WarmUpAsync(
                SceneWarmupContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(SceneReadinessResult.Ready());
            }

            public Task<SceneReadinessResult> ValidateAsync(
                SceneInitializationContext context,
                CancellationToken cancellationToken = default)
            {
                return Task.FromResult(SceneReadinessResult.Ready());
            }
        }
    }
}
