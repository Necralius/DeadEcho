using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class GameplaySceneContextInitializer : ISceneInitializer
    {
        public int Order => 0;

        public Task InitializeAsync(
            SceneInitializationContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!context.Scene.IsValid())
                throw new InvalidOperationException("Gameplay scene is invalid.");

            if (context.Payload == null)
                throw new InvalidOperationException("Gameplay scene requires a transition payload.");

            progress?.Report(1f);
            return Task.CompletedTask;
        }
    }

    public sealed class PlaceholderGameplayWarmupStep : ISceneWarmupStep
    {
        public int Order => 0;
        public float Weight => 1f;

        public Task WarmupAsync(
            SceneWarmupContext context,
            IProgress<float> progress,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(1f);
            return Task.CompletedTask;
        }
    }
}
