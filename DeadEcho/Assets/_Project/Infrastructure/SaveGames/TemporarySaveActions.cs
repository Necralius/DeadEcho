using System;
using System.Threading.Tasks;
using Project.Core.Services;
using UnityEngine;

namespace Project.Infrastructure.SaveGames
{
    public sealed class TemporaryLoadGameService : ILoadGameService
    {
        public Task LoadAsync(string saveId)
        {
            Debug.Log($"[TemporaryLoadGameService] Load requested for save '{saveId}'. Integrate with the real save pipeline when available.");
            return Task.CompletedTask;
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
        public Task StartNewGameAsync()
        {
            Debug.Log("[TemporaryNewGameService] New game requested. Integrate with scene/gameplay bootstrap when available.");
            return Task.CompletedTask;
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
