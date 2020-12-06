using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VRNotifier.DTO;

namespace VRNotifier.Services
{
    public interface IVRPersistenceClient
    {
        Task<Result<IEnumerable<MediaDTO>>> GetSubscribedToMedia(string notificationEndpointIdentifier);
        Task<Result> Subscribe(AddSubscriptionsDTO addSubscriptionsDto);
        Task<Result> Unsubscribe(DeleteSubscriptionsDTO deleteSubscriptionsDto);
    }
}