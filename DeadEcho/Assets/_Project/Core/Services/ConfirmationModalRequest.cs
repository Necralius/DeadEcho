using System;

namespace Project.Core.Services
{
    public enum ModalVariant
    {
        Normal,
        Warning,
        Danger
    }

    public readonly struct ConfirmationModalRequest
    {
        public ConfirmationModalRequest(
            string title,
            string message,
            string confirmLabel = "Confirm",
            string cancelLabel = "Cancel",
            ModalVariant variant = ModalVariant.Normal,
            bool allowCancel = true)
        {
            Title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Title cannot be empty.", nameof(title)) : title;
            Message = message ?? string.Empty;
            ConfirmLabel = string.IsNullOrWhiteSpace(confirmLabel) ? "Confirm" : confirmLabel;
            CancelLabel = string.IsNullOrWhiteSpace(cancelLabel) ? "Cancel" : cancelLabel;
            Variant = variant;
            AllowCancel = allowCancel;
        }

        public string Title { get; }
        public string Message { get; }
        public string ConfirmLabel { get; }
        public string CancelLabel { get; }
        public ModalVariant Variant { get; }
        public bool AllowCancel { get; }
    }
}
