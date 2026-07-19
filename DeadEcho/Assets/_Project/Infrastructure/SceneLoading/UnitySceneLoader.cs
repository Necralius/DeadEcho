using Project.Core.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Infrastructure.SceneLoading
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        private AsyncOperation _currentOperation;

        public bool IsLoading => _currentOperation != null && !_currentOperation.isDone;
        public float Progress => _currentOperation?.progress ?? 0f;

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[UnitySceneLoader] Ignored scene load request with an empty scene name.");
                return;
            }

            Debug.Log($"[UnitySceneLoader] Loading scene '{sceneName}'.");
            _currentOperation = SceneManager.LoadSceneAsync(sceneName);
        }
    }
}
