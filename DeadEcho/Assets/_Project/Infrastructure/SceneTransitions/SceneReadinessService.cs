using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class SceneReadinessService : ISceneReadinessService
    {
        private readonly IReadOnlyList<ISceneInitializer> _initializers;

        public SceneReadinessService(IReadOnlyList<ISceneInitializer> initializers = null)
        {
            _initializers = initializers ?? new List<ISceneInitializer>();
        }

        public async Task InitializeAsync(string sceneName, ScenePayload payload, CancellationToken cancellationToken = default)
        {
            foreach (ISceneInitializer initializer in _initializers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await initializer.InitializeAsync(payload, cancellationToken);
            }
        }

        public async Task WarmUpAsync(string sceneName, CancellationToken cancellationToken = default)
        {
            foreach (ISceneInitializer initializer in _initializers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await initializer.WarmUpAsync(cancellationToken);
            }
        }
    }
}
