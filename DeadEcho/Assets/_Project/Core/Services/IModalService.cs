using System.Threading.Tasks;

namespace Project.Core.Services
{
    public interface IModalService
    {
        Task<bool> ConfirmAsync(ConfirmationModalRequest request);
        Task InformationAsync(string title, string message, string confirmLabel = "OK");
        Task ErrorAsync(string title, string message, string confirmLabel = "OK");
    }
}
