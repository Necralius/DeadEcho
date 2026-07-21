using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class SceneReadinessService : ISceneReadinessService
    {
        private readonly ISceneScopeReadinessProvider _readinessProvider;

        public SceneReadinessService(ISceneScopeReadinessProvider readinessProvider)
        {
            _readinessProvider = readinessProvider ?? throw new ArgumentNullException(nameof(readinessProvider));
        }

        public async Task<SceneReadinessResult> InitializeAsync(
            SceneInitializationContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken = default)
        {
            if (!context.Scene.IsValid())
                return SceneReadinessResult.Failed(new SceneInitializationError("Scene", "Loaded scene is invalid."));

            IReadOnlyList<ISceneInitializer> initializers = _readinessProvider
                .GetInitializers(context.Scene)
                .OrderBy(initializer => initializer.Order)
                .ToArray();

            if (initializers.Count == 0)
            {
                progress?.Report(1f);
                return SceneReadinessResult.Ready();
            }

            return await RunOrderedAsync(
                initializers,
                initializer => initializer.GetType().Name,
                (initializer, stepProgress, token) => initializer.InitializeAsync(context, stepProgress, token),
                progress,
                cancellationToken);
        }

        public async Task<SceneReadinessResult> WarmUpAsync(
            SceneWarmupContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken = default)
        {
            if (!context.Scene.IsValid())
                return SceneReadinessResult.Failed(new SceneInitializationError("Scene", "Loaded scene is invalid."));

            IReadOnlyList<ISceneWarmupStep> warmupSteps = _readinessProvider
                .GetWarmupSteps(context.Scene)
                .OrderBy(step => step.Order)
                .ToArray();

            if (warmupSteps.Count == 0)
            {
                progress?.Report(1f);
                return SceneReadinessResult.Ready();
            }

            float totalWeight = warmupSteps.Sum(step => Math.Max(0.001f, step.Weight));
            float completedWeight = 0f;

            foreach (ISceneWarmupStep step in warmupSteps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float stepWeight = Math.Max(0.001f, step.Weight);
                var stepProgress = new Progress<float>(value =>
                {
                    float clamped = Math.Max(0f, Math.Min(1f, value));
                    progress?.Report((completedWeight + stepWeight * clamped) / totalWeight);
                });

                try
                {
                    await step.WarmupAsync(context, stepProgress, cancellationToken);
                }
                catch (Exception exception) when (!(exception is OperationCanceledException))
                {
                    return SceneReadinessResult.Failed(
                        new SceneInitializationError(step.GetType().Name, exception.Message, exception));
                }

                completedWeight += stepWeight;
                progress?.Report(completedWeight / totalWeight);
            }

            return SceneReadinessResult.Ready();
        }

        public Task<SceneReadinessResult> ValidateAsync(
            SceneInitializationContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!context.Scene.IsValid())
                return Task.FromResult(SceneReadinessResult.Failed(new SceneInitializationError("Scene", "Loaded scene is invalid.")));

            if (!context.Scene.isLoaded)
                return Task.FromResult(SceneReadinessResult.Pending(context.Scene.name));

            return Task.FromResult(SceneReadinessResult.Ready());
        }

        private static async Task<SceneReadinessResult> RunOrderedAsync<TStep>(
            IReadOnlyList<TStep> steps,
            Func<TStep, string> stepName,
            Func<TStep, IProgress<float>, CancellationToken, Task> runStep,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int stepIndex = i;
                var stepProgress = new Progress<float>(value =>
                {
                    float clamped = Math.Max(0f, Math.Min(1f, value));
                    progress?.Report((stepIndex + clamped) / steps.Count);
                });

                try
                {
                    await runStep(steps[i], stepProgress, cancellationToken);
                }
                catch (Exception exception) when (!(exception is OperationCanceledException))
                {
                    return SceneReadinessResult.Failed(
                        new SceneInitializationError(stepName(steps[i]), exception.Message, exception));
                }

                progress?.Report((i + 1f) / steps.Count);
            }

            return SceneReadinessResult.Ready();
        }
    }
}
