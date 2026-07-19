namespace Project.Core.Services
{
    public interface ISceneLoader
    {
        bool IsLoading { get; }
        float Progress { get; }

        void LoadScene(string sceneName);
    }
}
