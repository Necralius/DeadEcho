namespace Project.Core.Services
{
    public interface IUiService
    {
        bool CanGoBack { get; }

        void Show(UiScreenId screenId);
        void Replace(UiScreenId screenId);
        void Back();
        void CloseCurrent();
    }
}
