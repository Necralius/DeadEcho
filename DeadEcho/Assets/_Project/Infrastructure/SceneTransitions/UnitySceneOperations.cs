using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Infrastructure.SceneTransitions
{
    public sealed class UnitySceneOperations : IUnitySceneOperations
    {
        public string ActiveSceneName => SceneManager.GetActiveScene().name;

        public bool CanLoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return false;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(i);
                string name = Path.GetFileNameWithoutExtension(path);
                if (name == sceneName || path == sceneName)
                    return true;
            }

            return false;
        }

        public ISceneLoadOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            return operation == null ? null : new UnitySceneLoadOperation(operation);
        }

        public bool SetActiveScene(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && SceneManager.SetActiveScene(scene);
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            AsyncOperation operation = SceneManager.UnloadSceneAsync(scene);
            while (operation != null && !operation.isDone)
                await Task.Yield();
        }

        private sealed class UnitySceneLoadOperation : ISceneLoadOperation
        {
            private readonly AsyncOperation _operation;

            public UnitySceneLoadOperation(AsyncOperation operation)
            {
                _operation = operation;
            }

            public float Progress => _operation.progress;
            public bool IsDone => _operation.isDone;

            public bool AllowSceneActivation
            {
                get => _operation.allowSceneActivation;
                set => _operation.allowSceneActivation = value;
            }
        }
    }
}
