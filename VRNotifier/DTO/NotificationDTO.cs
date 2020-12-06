using System.Collections.Generic;

namespace VRNotifier.DTO
{
    public class NotificationDTO
    {
        public string Message { get; set; }
        public List<string> NotificationEndpointIdentifiers { get; set; }
    }
}