using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public interface INewGameService
    {
        Task StartNewGameAsync();
    }

    public interface ILoadGameService
    {
        Task LoadAsync(string saveId);
    }

    public interface IDeleteSaveService
    {
        Task DeleteAsync(string saveId);
    }

    public interface IApplicationQuitService
    {
        bool IsQuitAvailable { get; }
        void Quit();
    }

    public readonly struct SaveGameSummary
    {
        public SaveGameSummary(
            string saveId,
            string displayName,
            DateTime savedAt,
            TimeSpan playTime,
            string location,
            bool isValid = true)
        {
            SaveId = saveId ?? string.Empty;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Save" : displayName;
            SavedAt = savedAt;
            PlayTime = playTime;
            Location = location ?? string.Empty;
            IsValid = isValid;
        }

        public string SaveId { get; }
        public string DisplayName { get; }
        public DateTime SavedAt { get; }
        public TimeSpan PlayTime { get; }
        public string Location { get; }
        public bool IsValid { get; }
    }

    public interface ICreditsContentProvider
    {
        IReadOnlyList<CreditsSection> GetSections();
    }

    public readonly struct CreditsSection
    {
        public CreditsSection(string title, IReadOnlyList<string> names)
        {
            Title = string.IsNullOrWhiteSpace(title) ? "Credits" : title;
            Names = names ?? Array.Empty<string>();
        }

        public string Title { get; }
        public IReadOnlyList<string> Names { get; }
    }
}
