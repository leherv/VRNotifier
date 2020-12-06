using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VRNotifier.DTO;

namespace VRNotifier.Services
{
    public interface INotificationService
    {
        public Task<Result> Notify(NotificationDTO notificationDto);
    }
}