using System;
using Project.Core.Services;
using Project.UI.Navigation;
using UnityEngine.UIElements;

namespace Project.UI.MainMenu.Controllers
{
    public sealed class CreditsScreenController : IUiScreenController, IUiNavigationRequestSource
    {
        private readonly Button _backButton;

        public CreditsScreenController(ICreditsContentProvider contentProvider)
        {
            Root = new VisualElement { name = "credits-screen" };
            Root.AddToClassList("ui-screen");
            Root.Add(new Label("Credits") { name = "screen-title" });
            Root.Q<Label>("screen-title").AddToClassList("screen-title");

            var scroll = new ScrollView();
            scroll.AddToClassList("credits-scroll");
            foreach (CreditsSection section in contentProvider.GetSections())
            {
                var title = new Label(section.Title);
                title.AddToClassList("settings-dirty");
                scroll.Add(title);
                foreach (string name in section.Names)
                    scroll.Add(new Label(name));
            }

            Root.Add(scroll);
            _backButton = new Button(() => BackRequested?.Invoke()) { text = "Back" };
            _backButton.AddToClassList("button-secondary");
            _backButton.AddToClassList("menu-button");
            Root.Add(_backButton);
            DefaultFocus = _backButton;
        }

        public event Action<UiScreenId> ShowRequested { add { } remove { } }
        public event Action<UiScreenId> ReplaceRequested { add { } remove { } }
        public event Action BackRequested;
        public UiScreenId ScreenId => UiScreenId.Credits;
        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; }
        public void Open() => Root.style.display = DisplayStyle.Flex;
        public void Close() => Root.RemoveFromHierarchy();
        public void Dispose() => Close();
    }
}
