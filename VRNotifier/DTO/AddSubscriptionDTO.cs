namespace VRNotifier.DTO
{
    public class AddSubscriptionDTO
    {
        public string MediaName { get; set; }

        public AddSubscriptionDTO() {}

        public AddSubscriptionDTO(string mediaName)
        {
            MediaName = mediaName;
        }
    }
}