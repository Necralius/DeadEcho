using System;
using Project.Core.Services;
using Project.UI.Modals;
using Project.UI.Navigation;
using Project.UI.Runtime;
using UnityEngine;

namespace Project.UI.Services
{
    public sealed class UiService : IUiService, IDisposable
    {
        private readonly IUiRoot _root;
        private readonly UiScreenCatalog _catalog;
        private readonly UiNavigationStack _navigationStack;
        private readonly ModalService _modalService;
        private IUiScreenController _currentScreen;

        public UiService(
            IUiRoot root,
            UiScreenCatalog catalog,
            UiNavigationStack navigationStack,
            ModalService modalService)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _navigationStack = navigationStack ?? throw new ArgumentNullException(nameof(navigationStack));
            _modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));

            _root.CancelRequested += OnCancelRequested;
            _modalService.ModalClosed += OnModalClosed;
        }

        public bool CanGoBack => !_modalService.IsModalOpen && _navigationStack.CanGoBack;
        public UiScreenId? CurrentScreenId => _currentScreen?.ScreenId;

        public void Show(UiScreenId screenId)
        {
            if (_modalService.IsModalOpen)
                return;

            if (_currentScreen != null)
            {
                DetachNavigationRequests(_currentScreen);
                _currentScreen.Root.RemoveFromHierarchy();
                _navigationStack.Push(_currentScreen);
            }

            SetCurrentScreen(_catalog.Create(screenId));
        }

        public void Replace(UiScreenId screenId)
        {
            if (_modalService.IsModalOpen)
                return;

            CloseCurrentInternal(dispose: true);
            SetCurrentScreen(_catalog.Create(screenId));
        }

        public void Back()
        {
            if (_modalService.TryCancelActiveModal())
                return;

            if (!_navigationStack.TryPop(out IUiScreenController previousScreen))
                return;

            CloseCurrentInternal(dispose: true);
            SetCurrentScreen(previousScreen);
        }

        public void CloseCurrent()
        {
            if (_modalService.IsModalOpen)
                return;

            CloseCurrentInternal(dispose: true);
        }

        public void Dispose()
        {
            _root.CancelRequested -= OnCancelRequested;
            _modalService.ModalClosed -= OnModalClosed;
            CloseCurrentInternal(dispose: true);
            _navigationStack.Clear();
        }

        private void SetCurrentScreen(IUiScreenController screen)
        {
            _currentScreen = screen ?? throw new ArgumentNullException(nameof(screen));
            _root.ScreenLayer.Clear();
            _root.ScreenLayer.Add(_currentScreen.Root);
            AttachNavigationRequests(_currentScreen);
            _currentScreen.Open();
            _root.Focus(_currentScreen.DefaultFocus);
            Debug.Log($"[UiService] Showing screen '{screen.ScreenId}'.");
        }

        private void CloseCurrentInternal(bool dispose)
        {
            if (_currentScreen == null)
                return;

            DetachNavigationRequests(_currentScreen);

            if (dispose)
                _currentScreen.Dispose();
            else
                _currentScreen.Close();

            _currentScreen = null;
            _root.ScreenLayer.Clear();
        }

        private void OnCancelRequested()
        {
            Back();
        }

        private void OnModalClosed(bool _)
        {
            if (_currentScreen != null)
                _root.Focus(_currentScreen.DefaultFocus);
        }

        private void AttachNavigationRequests(IUiScreenController screen)
        {
            if (screen is not IUiNavigationRequestSource requestSource)
                return;

            requestSource.ShowRequested += Show;
            requestSource.ReplaceRequested += Replace;
            requestSource.BackRequested += Back;
        }

        private void DetachNavigationRequests(IUiScreenController screen)
        {
            if (screen is not IUiNavigationRequestSource requestSource)
                return;

            requestSource.ShowRequested -= Show;
            requestSource.ReplaceRequested -= Replace;
            requestSource.BackRequested -= Back;
        }
    }
}
