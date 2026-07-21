using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Project.Core.Services;
using Project.Infrastructure.SceneTransitions;
using UnityEngine.SceneManagement;

namespace Project.Tests.EditMode.SceneTransitions
{
    public sealed class SceneReadinessServiceTests
    {
        [Test]
        public async Task RunsInitializersInOrder()
        {
            List<string> events = new List<string>();
            var provider = new FakeProvider(
                new ISceneInitializer[]
                {
                    new RecordingInitializer(20, "second", events),
                    new RecordingInitializer(10, "first", events)
                },
                Array.Empty<ISceneWarmupStep>());
            SceneReadinessService service = new SceneReadinessService(provider);

            SceneReadinessResult result = await service.InitializeAsync(CreateContext(), null);

            Assert.IsTrue(result.IsReady);
            CollectionAssert.AreEqual(new[] { "first", "second" }, events);
        }

        [Test]
        public async Task ReportsInitializerProgressAcrossSteps()
        {
            List<float> progress = new List<float>();
            var provider = new FakeProvider(
                new ISceneInitializer[]
                {
                    new RecordingInitializer(0, "a", null),
                    new RecordingInitializer(1, "b", null)
                },
                Array.Empty<ISceneWarmupStep>());
            SceneReadinessService service = new SceneReadinessService(provider);

            await service.InitializeAsync(CreateContext(), new Progress<float>(progress.Add));

            Assert.That(progress.Last(), Is.EqualTo(1f).Within(0.001f));
            Assert.That(progress, Has.Some.GreaterThanOrEqualTo(0.5f));
        }

        [Test]
        public async Task RespectsWarmupWeights()
        {
            List<float> progress = new List<float>();
            var provider = new FakeProvider(
                Array.Empty<ISceneInitializer>(),
                new ISceneWarmupStep[]
                {
                    new RecordingWarmupStep(0, 1f, "small", null),
                    new RecordingWarmupStep(1, 3f, "large", null)
                });
            SceneReadinessService service = new SceneReadinessService(provider);

            await service.WarmUpAsync(CreateWarmupContext(), new Progress<float>(progress.Add));

            Assert.That(progress.Last(), Is.EqualTo(1f).Within(0.001f));
            Assert.That(progress, Has.Some.EqualTo(0.25f).Within(0.001f));
        }

        [Test]
        public async Task StopsOnInitializerError()
        {
            var provider = new FakeProvider(
                new ISceneInitializer[] { new FailingInitializer() },
                Array.Empty<ISceneWarmupStep>());
            SceneReadinessService service = new SceneReadinessService(provider);

            SceneReadinessResult result = await service.InitializeAsync(CreateContext(), null);

            Assert.IsFalse(result.IsReady);
            Assert.AreEqual(1, result.Errors.Count);
        }

        [Test]
        public async Task ValidateDetectsPendingScene()
        {
            SceneReadinessService service = new SceneReadinessService(
                new FakeProvider(Array.Empty<ISceneInitializer>(), Array.Empty<ISceneWarmupStep>()));
            var context = new SceneInitializationContext(default, null, new SceneTransitionRequest("Missing"));

            SceneReadinessResult result = await service.ValidateAsync(context);

            Assert.IsFalse(result.IsReady);
            Assert.AreEqual(1, result.Errors.Count);
        }

        private static SceneInitializationContext CreateContext()
        {
            return new SceneInitializationContext(
                SceneManager.GetActiveScene(),
                new ScenePayload("Test"),
                new SceneTransitionRequest("TestScene"));
        }

        private static SceneWarmupContext CreateWarmupContext()
        {
            return new SceneWarmupContext(
                SceneManager.GetActiveScene(),
                new ScenePayload("Test"),
                new SceneTransitionRequest("TestScene"));
        }

        private sealed class FakeProvider : ISceneScopeReadinessProvider
        {
            private readonly IReadOnlyList<ISceneInitializer> _initializers;
            private readonly IReadOnlyList<ISceneWarmupStep> _warmupSteps;

            public FakeProvider(
                IReadOnlyList<ISceneInitializer> initializers,
                IReadOnlyList<ISceneWarmupStep> warmupSteps)
            {
                _initializers = initializers;
                _warmupSteps = warmupSteps;
            }

            public bool HasSceneScope(Scene scene) => true;
            public IReadOnlyList<ISceneInitializer> GetInitializers(Scene scene) => _initializers;
            public IReadOnlyList<ISceneWarmupStep> GetWarmupSteps(Scene scene) => _warmupSteps;
        }

        private sealed class RecordingInitializer : ISceneInitializer
        {
            private readonly string _name;
            private readonly List<string> _events;

            public RecordingInitializer(int order, string name, List<string> events)
            {
                Order = order;
                _name = name;
                _events = events;
            }

            public int Order { get; }

            public Task InitializeAsync(
                SceneInitializationContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken)
            {
                _events?.Add(_name);
                progress?.Report(1f);
                return Task.CompletedTask;
            }
        }

        private sealed class RecordingWarmupStep : ISceneWarmupStep
        {
            private readonly string _name;
            private readonly List<string> _events;

            public RecordingWarmupStep(int order, float weight, string name, List<string> events)
            {
                Order = order;
                Weight = weight;
                _name = name;
                _events = events;
            }

            public int Order { get; }
            public float Weight { get; }

            public Task WarmupAsync(
                SceneWarmupContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken)
            {
                _events?.Add(_name);
                progress?.Report(1f);
                return Task.CompletedTask;
            }
        }

        private sealed class FailingInitializer : ISceneInitializer
        {
            public int Order => 0;

            public Task InitializeAsync(
                SceneInitializationContext context,
                IProgress<float> progress,
                CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Broken initializer.");
            }
        }
    }
}
