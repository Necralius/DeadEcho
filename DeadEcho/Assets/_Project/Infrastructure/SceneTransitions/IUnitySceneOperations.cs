using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Project.Infrastructure.SceneTransitions
{
    public interface IUnitySceneOperations
    {
        string ActiveSceneName { get; }
        bool CanLoadScene(string sceneName);
        ISceneLoadOperation LoadSceneAsync(string sceneName, LoadSceneMode mode);
        bool SetActiveScene(string sceneName);
        Task UnloadSceneAsync(string sceneName);
    }

    public interface ISceneLoadOperation
    {
        float Progress { get; }
        bool IsDone { get; }
        bool AllowSceneActivation { get; set; }
    }
}
