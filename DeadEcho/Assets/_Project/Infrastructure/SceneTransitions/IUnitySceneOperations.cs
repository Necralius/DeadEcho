using System.Threading.Tasks;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Project.Infrastructure.SceneTransitions
{
    public interface IUnitySceneOperations
    {
        string ActiveSceneName { get; }
        bool CanLoadScene(string sceneName);
        Scene GetScene(string sceneName);
        ISceneLoadOperation LoadSceneAsync(string sceneName, LoadSceneMode mode);
        bool SetActiveScene(string sceneName);
        Task UnloadSceneAsync(string sceneName);
        Task WaitForFramesAsync(int frameCount, bool includeEndOfFrame, CancellationToken cancellationToken = default);
    }

    public interface ISceneLoadOperation
    {
        float Progress { get; }
        bool IsDone { get; }
        bool AllowSceneActivation { get; set; }
    }
}
