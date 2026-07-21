using Project.Core.Services;

namespace Project.Infrastructure.SaveGames
{
    public sealed class NoSaveGameQuery : ISaveGameQuery
    {
        public bool HasAnySave()
        {
            return false;
        }

        public System.Collections.Generic.IReadOnlyList<SaveGameSummary> GetAvailableSaves()
        {
            return System.Array.Empty<SaveGameSummary>();
        }
    }
}
