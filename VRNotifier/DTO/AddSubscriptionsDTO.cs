using System.Collections.Generic;

namespace VRNotifier.DTO
{
    public class AddSubscriptionsDTO
    {
        public string NotificationEndpointIdentifier { get; set; }
        public List<AddSubscriptionDTO> Subscriptions { get; set; }

        public AddSubscriptionsDTO()
        {
        }

        public AddSubscriptionsDTO(string notificationEndpointIdentifier, List<AddSubscriptionDTO> subscriptions)
        {
            NotificationEndpointIdentifier = notificationEndpointIdentifier;
            Subscriptions = subscriptions;
        }
    }
}