using Project.Core.Services;
using Project.UI.Navigation;
using UnityEngine.UIElements;

namespace Project.UI.MainMenu.Controllers
{
    public sealed class TemporaryMenuScreenController : IUiScreenController
    {
        private readonly string _title;

        public TemporaryMenuScreenController(UiScreenId screenId, string title)
        {
            ScreenId = screenId;
            _title = title;
            Root = BuildRoot();
        }

        public UiScreenId ScreenId { get; }
        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; private set; }

        public void Open()
        {
            Root.style.display = DisplayStyle.Flex;
        }

        public void Close()
        {
            Root.RemoveFromHierarchy();
        }

        public void Dispose()
        {
            Close();
        }

        private VisualElement BuildRoot()
        {
            var root = new VisualElement { name = $"{ScreenId}-screen" };
            root.AddToClassList("ui-screen");
            root.pickingMode = PickingMode.Position;

            var title = new Label(_title) { name = "screen-title" };
            title.AddToClassList("screen-title");
            root.Add(title);

            var placeholder = new Label("Temporary screen");
            placeholder.AddToClassList("screen-placeholder");
            root.Add(placeholder);

            DefaultFocus = new Button { text = "Continue" };
            DefaultFocus.name = "default-action";
            root.Add(DefaultFocus);

            return root;
        }
    }
}
