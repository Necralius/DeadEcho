using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;

namespace Project.Infrastructure.Settings
{
    public sealed class GraphicsRevertService : IGraphicsRevertService
    {
        private readonly IGraphicsSettingsService _graphicsSettingsService;
        private readonly IModalService _modalService;

        public GraphicsRevertService(IGraphicsSettingsService graphicsSettingsService, IModalService modalService)
        {
            _graphicsSettingsService = graphicsSettingsService;
            _modalService = modalService;
        }

        public async Task<bool> ApplyWithConfirmationAsync(GraphicsSettingsSnapshot settings, CancellationToken cancellationToken = default)
        {
            GraphicsSettingsSnapshot previous = _graphicsSettingsService.Current;
            _graphicsSettingsService.Apply(settings);

            Task<bool> confirmTask = _modalService.ConfirmAsync(new ConfirmationModalRequest(
                "Keep graphics settings?",
                "Confirm within 10 seconds or the previous graphics settings will be restored.",
                "Keep",
                "Revert",
                ModalVariant.Warning));

            Task timeoutTask = Task.Delay(10000, cancellationToken);
            Task completed = await Task.WhenAny(confirmTask, timeoutTask);
            bool confirmed = completed == confirmTask && confirmTask.Result;
            if (!confirmed)
                _graphicsSettingsService.Apply(previous);

            return confirmed;
        }
    }
}
