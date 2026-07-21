namespace Project.Core.Services
{
    public interface ISaveGameQuery
    {
        bool HasAnySave();
        System.Collections.Generic.IReadOnlyList<SaveGameSummary> GetAvailableSaves();
    }
}
