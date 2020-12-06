using System.Collections.Generic;

 namespace VRNotifier.DTO
{
    public class DeleteSubscriptionsDTO
    {
        public string NotificationEndpointIdentifier { get; set; }
        public List<DeleteSubscriptionDTO> Subscriptions { get; set; }

        public DeleteSubscriptionsDTO()
        {
        }

        public DeleteSubscriptionsDTO(string notificationEndpointIdentifier, List<DeleteSubscriptionDTO> subscriptions)
        {
            NotificationEndpointIdentifier = notificationEndpointIdentifier;
            Subscriptions = subscriptions;
        }
    }
}