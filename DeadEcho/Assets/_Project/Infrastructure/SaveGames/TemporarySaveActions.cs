using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Project.Core.Services;
using UnityEngine;

namespace Project.Infrastructure.SaveGames
{
    public sealed class TemporaryLoadGameService : ILoadGameService
    {
        private const string TemporaryGameplaySceneName = "OutdoorsScene";
        private readonly ISceneTransitionService _sceneTransitionService;

        public TemporaryLoadGameService(ISceneTransitionService sceneTransitionService)
        {
            _sceneTransitionService = sceneTransitionService;
        }

        public async Task LoadAsync(string saveId)
        {
            Debug.Log($"[TemporaryLoadGameService] Load requested for save '{saveId}'. Integrate with the real save pipeline when available.");
            var payload = new ScenePayload(
                "LoadGame",
                new Dictionary<string, string> { { "SaveId", saveId ?? string.Empty } });

            SceneTransitionResult result = await _sceneTransitionService.TransitionAsync(
                new SceneTransitionRequest(TemporaryGameplaySceneName) { Payload = payload });

            if (!result.Succeeded)
                throw new InvalidOperationException(result.Error?.Message ?? "Load scene transition failed.");
        }
    }

    public sealed class TemporaryDeleteSaveService : IDeleteSaveService
    {
        public Task DeleteAsync(string saveId)
        {
            Debug.Log($"[TemporaryDeleteSaveService] Delete requested for save '{saveId}'. Integrate with the real save pipeline when available.");
            return Task.CompletedTask;
        }
    }

    public sealed class TemporaryNewGameService : INewGameService
    {
        private const string TemporaryGameplaySceneName = "OutdoorsScene";
        private readonly ISceneTransitionService _sceneTransitionService;

        public TemporaryNewGameService(ISceneTransitionService sceneTransitionService)
        {
            _sceneTransitionService = sceneTransitionService;
        }

        public async Task StartNewGameAsync()
        {
            Debug.Log("[TemporaryNewGameService] New game requested. Integrate with scene/gameplay bootstrap when available.");
            SceneTransitionResult result = await _sceneTransitionService.TransitionAsync(
                new SceneTransitionRequest(TemporaryGameplaySceneName)
                {
                    Payload = new ScenePayload("NewGame")
                });

            if (!result.Succeeded)
                throw new InvalidOperationException(result.Error?.Message ?? "New game scene transition failed.");
        }
    }

    public sealed class UnityApplicationQuitService : IApplicationQuitService
    {
        public bool IsQuitAvailable => true;

        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("[UnityApplicationQuitService] Quit requested in Editor.");
#else
            Application.Quit();
#endif
        }
    }

    public sealed class StaticCreditsContentProvider : ICreditsContentProvider
    {
        public System.Collections.Generic.IReadOnlyList<CreditsSection> GetSections()
        {
            return new[]
            {
                new CreditsSection("Development", new[] { "Dead Echo Team" }),
                new CreditsSection("Special Thanks", new[] { "Community and playtesters" })
            };
        }
    }
}
