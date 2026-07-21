using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.UI.Runtime
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class UiRoot : MonoBehaviour, IUiRoot
    {
        public const string ScreenLayerName = "screen-layer";
        public const string ModalLayerName = "modal-layer";
        public const string InteractionBlockerName = "interaction-blocker";
        private const string SharedThemePath = "UI/Shared/MainMenuTheme";

        [SerializeField] private UIDocument document;
        private bool _initialized;

        public VisualElement Root { get; private set; }
        public VisualElement ScreenLayer { get; private set; }
        public VisualElement ModalLayer { get; private set; }
        public VisualElement InteractionBlocker { get; private set; }

        public event Action CancelRequested;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized && Root != null)
                Root.UnregisterCallback<KeyDownEvent>(OnKeyDown);

            if (document == null)
                document = GetComponent<UIDocument>();

            Root = document.rootVisualElement;
            Root.style.flexGrow = 1;
            Root.style.width = Length.Percent(100);
            Root.style.height = Length.Percent(100);
            ApplyTheme();
            ScreenLayer = EnsureLayer(ScreenLayerName);
            InteractionBlocker = EnsureLayer(InteractionBlockerName);
            ModalLayer = EnsureLayer(ModalLayerName);

            StretchToPanel(ScreenLayer);
            StretchToPanel(InteractionBlocker);
            StretchToPanel(ModalLayer);
            InteractionBlocker.pickingMode = PickingMode.Position;
            InteractionBlocker.style.display = DisplayStyle.None;
            ModalLayer.style.display = DisplayStyle.None;

            Root.focusable = true;
            Root.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _initialized = true;
        }

        public void Focus(VisualElement element)
        {
            (element ?? Root)?.Focus();
        }

        private VisualElement EnsureLayer(string layerName)
        {
            VisualElement layer = Root.Q<VisualElement>(layerName);
            if (layer != null)
                return layer;

            layer = new VisualElement { name = layerName };
            layer.AddToClassList(layerName);
            Root.Add(layer);
            return layer;
        }

        private static void StretchToPanel(VisualElement element)
        {
            element.style.position = Position.Absolute;
            element.style.left = 0;
            element.style.right = 0;
            element.style.top = 0;
            element.style.bottom = 0;
        }

        private void ApplyTheme()
        {
            StyleSheet theme = Resources.Load<StyleSheet>(SharedThemePath);
            if (theme != null)
                Root.styleSheets.Add(theme);
        }

        private void OnDestroy()
        {
            Root?.UnregisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
                return;

            CancelRequested?.Invoke();
            evt.StopPropagation();
        }
    }
}
