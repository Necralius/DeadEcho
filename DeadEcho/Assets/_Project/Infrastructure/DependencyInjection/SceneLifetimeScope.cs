using VContainer;
using VContainer.Unity;
using Project.Core.Services;
using Project.Infrastructure.SceneTransitions;

namespace Project.Infrastructure.DependencyInjection
{
    public sealed class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameplaySceneContextInitializer>(Lifetime.Scoped).As<ISceneInitializer>();
            builder.Register<PlaceholderGameplayWarmupStep>(Lifetime.Scoped).As<ISceneWarmupStep>();
        }
    }
}
