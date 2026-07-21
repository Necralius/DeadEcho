using System;
using Project.Core.Services;
using Project.UI.Navigation;
using UnityEngine.UIElements;

namespace Project.UI.MainMenu.Controllers
{
    public sealed class ExtrasScreenController : IUiScreenController, IUiNavigationRequestSource
    {
        public ExtrasScreenController()
        {
            Root = new VisualElement { name = "extras-screen" };
            Root.AddToClassList("ui-screen");
            Root.Add(new Label("Extras") { name = "screen-title" });
            Root.Q<Label>("screen-title").AddToClassList("screen-title");

            var grid = new VisualElement();
            grid.AddToClassList("extras-grid");
            foreach (string item in new[] { "Bestiary", "Lore", "Gallery", "Achievements", "Statistics" })
            {
                var card = new VisualElement();
                card.AddToClassList("ui-panel");
                card.Add(new Label(item));
                card.Add(new Label("Not available yet.") { name = "screen-placeholder" });
                card.Q<Label>("screen-placeholder").AddToClassList("screen-placeholder");
                grid.Add(card);
            }

            Root.Add(grid);
            var back = new Button(() => BackRequested?.Invoke()) { text = "Back" };
            back.AddToClassList("button-secondary");
            back.AddToClassList("menu-button");
            Root.Add(back);
            DefaultFocus = back;
        }

        public event Action<UiScreenId> ShowRequested { add { } remove { } }
        public event Action<UiScreenId> ReplaceRequested { add { } remove { } }
        public event Action BackRequested;
        public UiScreenId ScreenId => UiScreenId.Extras;
        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; }
        public void Open() => Root.style.display = DisplayStyle.Flex;
        public void Close() => Root.RemoveFromHierarchy();
        public void Dispose() => Close();
    }
}
