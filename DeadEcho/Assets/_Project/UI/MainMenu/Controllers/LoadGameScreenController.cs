using System;
using System.Threading.Tasks;
using Project.Core.Services;
using Project.UI.Navigation;
using UnityEngine.UIElements;

namespace Project.UI.MainMenu.Controllers
{
    public sealed class LoadGameScreenController : IUiScreenController, IUiNavigationRequestSource
    {
        private readonly ISaveGameQuery _saveGameQuery;
        private readonly ILoadGameService _loadGameService;
        private readonly IDeleteSaveService _deleteSaveService;
        private readonly IModalService _modalService;
        private readonly VisualElement _list;
        private readonly Button _backButton;
        private bool _busy;

        public LoadGameScreenController(ISaveGameQuery saveGameQuery, ILoadGameService loadGameService, IDeleteSaveService deleteSaveService, IModalService modalService)
        {
            _saveGameQuery = saveGameQuery;
            _loadGameService = loadGameService;
            _deleteSaveService = deleteSaveService;
            _modalService = modalService;

            Root = new VisualElement { name = "load-game-screen" };
            Root.AddToClassList("ui-screen");
            Root.AddToClassList("menu-list-screen");
            Root.Add(new Label("Load Game") { name = "screen-title" });
            Root.Q<Label>("screen-title").AddToClassList("screen-title");
            _list = new VisualElement { name = "save-list" };
            _list.AddToClassList("save-list");
            Root.Add(_list);
            _backButton = new Button(() => BackRequested?.Invoke()) { text = "Back" };
            _backButton.AddToClassList("button-secondary");
            _backButton.AddToClassList("menu-button");
            Root.Add(_backButton);
            DefaultFocus = _backButton;
        }

        public event Action<UiScreenId> ShowRequested { add { } remove { } }
        public event Action<UiScreenId> ReplaceRequested { add { } remove { } }
        public event Action BackRequested;
        public UiScreenId ScreenId => UiScreenId.LoadGame;
        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; private set; }

        public void Open()
        {
            Root.style.display = DisplayStyle.Flex;
            Refresh();
        }

        public void Close() => Root.RemoveFromHierarchy();
        public void Dispose() => Close();

        private void Refresh()
        {
            _list.Clear();
            var saves = _saveGameQuery.GetAvailableSaves();
            if (saves.Count == 0)
            {
                var empty = new Label("No saves found.");
                empty.AddToClassList("screen-placeholder");
                _list.Add(empty);
                DefaultFocus = _backButton;
                return;
            }

            foreach (SaveGameSummary save in saves)
                AddSaveRow(save);
        }

        private void AddSaveRow(SaveGameSummary save)
        {
            var row = new VisualElement();
            row.AddToClassList("save-row");
            row.Add(new Label(save.DisplayName));
            row.Add(new Label($"{save.SavedAt:g}  {save.PlayTime:g}  {save.Location}"));
            if (!save.IsValid)
                row.Add(new Label("Invalid or corrupted save"));

            Button load = new Button(() => RunOnce(() => Load(save))) { text = "Load" };
            load.SetEnabled(save.IsValid);
            load.AddToClassList("button-primary");
            load.AddToClassList("menu-button");
            Button delete = new Button(() => RunOnce(() => Delete(save))) { text = "Delete" };
            delete.AddToClassList("button-danger");
            delete.AddToClassList("menu-button");
            row.Add(load);
            row.Add(delete);
            _list.Add(row);
            DefaultFocus ??= load;
        }

        private async void RunOnce(Func<Task> operation)
        {
            if (_busy)
                return;

            _busy = true;
            try { await operation(); }
            finally { _busy = false; }
        }

        private async Task Load(SaveGameSummary save)
        {
            try { await _loadGameService.LoadAsync(save.SaveId); }
            catch (Exception exception) { await _modalService.ErrorAsync("Load Failed", exception.Message); }
        }

        private async Task Delete(SaveGameSummary save)
        {
            bool confirmed = await _modalService.ConfirmAsync(new ConfirmationModalRequest("Delete Save", "This action cannot be undone.", "Delete", "Cancel", ModalVariant.Danger));
            if (!confirmed)
                return;
            await _deleteSaveService.DeleteAsync(save.SaveId);
            Refresh();
        }
    }
}
