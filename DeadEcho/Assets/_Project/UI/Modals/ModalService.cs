using System;
using System.Threading.Tasks;
using Project.Core.Services;
using Project.UI.Runtime;
using UnityEngine.UIElements;

namespace Project.UI.Modals
{
    public sealed class ModalService : IModalService
    {
        private readonly IUiRoot _root;
        private ConfirmationModalController _activeModal;
        private TaskCompletionSource<bool> _activeCompletion;

        public ModalService(IUiRoot root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        public bool IsModalOpen => _activeModal != null;
        public event Action<bool> ModalClosed;

        public Task<bool> ConfirmAsync(ConfirmationModalRequest request)
        {
            if (_activeModal != null)
                throw new InvalidOperationException("A modal is already open.");

            _activeCompletion = new TaskCompletionSource<bool>();
            _activeModal = new ConfirmationModalController(request, Complete);

            _root.ModalLayer.Clear();
            _root.ModalLayer.Add(_activeModal.Root);
            _root.ModalLayer.style.display = DisplayStyle.Flex;
            _root.InteractionBlocker.style.display = DisplayStyle.Flex;
            _root.Focus(_activeModal.DefaultFocus);

            return _activeCompletion.Task;
        }

        public async Task InformationAsync(string title, string message, string confirmLabel = "OK")
        {
            await ConfirmAsync(new ConfirmationModalRequest(title, message, confirmLabel, string.Empty, ModalVariant.Normal, false));
        }

        public async Task ErrorAsync(string title, string message, string confirmLabel = "OK")
        {
            await ConfirmAsync(new ConfirmationModalRequest(title, message, confirmLabel, string.Empty, ModalVariant.Danger, false));
        }

        public bool TryCancelActiveModal()
        {
            return TryCompleteActiveModal(false);
        }

        public bool TryCompleteActiveModal(bool result)
        {
            if (_activeModal == null)
                return false;

            if (result)
                _activeModal.Confirm();
            else
                _activeModal.Cancel();

            return true;
        }

        private void Complete(bool result)
        {
            ConfirmationModalController modal = _activeModal;
            TaskCompletionSource<bool> completion = _activeCompletion;

            _activeModal = null;
            _activeCompletion = null;

            modal?.Dispose();
            _root.ModalLayer.Clear();
            _root.ModalLayer.style.display = DisplayStyle.None;
            _root.InteractionBlocker.style.display = DisplayStyle.None;

            completion?.TrySetResult(result);
            ModalClosed?.Invoke(result);
        }
    }
}
