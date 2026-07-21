using System.Threading;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public interface ISceneTransitionService
    {
        bool IsTransitioning { get; }
        SceneTransitionStatus CurrentStatus { get; }

        Task<SceneTransitionResult> TransitionAsync(
            SceneTransitionRequest request,
            CancellationToken cancellationToken = default);
    }

    public interface ISceneLoadingView
    {
        Task ShowAsync(CancellationToken cancellationToken = default);
        void SetProgress(float normalizedProgress);
        void SetStatus(string status);
        void SetTip(string tip);
        void ShowError(SceneTransitionError error);
        Task HideAsync(CancellationToken cancellationToken = default);
    }

    public interface ISceneReadinessService
    {
        Task InitializeAsync(string sceneName, ScenePayload payload, CancellationToken cancellationToken = default);
        Task WarmUpAsync(string sceneName, CancellationToken cancellationToken = default);
    }

    public interface ISceneInitializer
    {
        Task InitializeAsync(ScenePayload payload, CancellationToken cancellationToken = default);
        Task WarmUpAsync(CancellationToken cancellationToken = default);
    }

    public interface ISceneGameplayGate
    {
        bool IsGameplayBlocked { get; }
        void Block();
        void Release();
    }

    public interface IScenePayloadStore
    {
        bool HasPayload { get; }
        ScenePayload Peek();
        ScenePayload Consume();
        void Store(ScenePayload payload);
        void Clear();
    }
}
