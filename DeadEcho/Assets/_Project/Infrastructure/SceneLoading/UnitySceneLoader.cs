using System.Threading.Tasks;
using Project.Core.Services;
using UnityEngine;

namespace Project.Infrastructure.SceneLoading
{
    public sealed class UnitySceneLoader : ISceneLoader
    {
        private readonly ISceneTransitionService _sceneTransitionService;
        private Task<SceneTransitionResult> _currentTransition;

        public UnitySceneLoader(ISceneTransitionService sceneTransitionService)
        {
            _sceneTransitionService = sceneTransitionService;
        }

        public bool IsLoading => _sceneTransitionService.IsTransitioning;
        public float Progress => _currentTransition == null || _currentTransition.IsCompleted ? 1f : 0f;

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("[UnitySceneLoader] Ignored scene load request with an empty scene name.");
                return;
            }

            Debug.Log($"[UnitySceneLoader] Delegating scene load '{sceneName}' to SceneTransitionService.");
            _currentTransition = _sceneTransitionService.TransitionAsync(new SceneTransitionRequest(sceneName));
        }
    }
}
