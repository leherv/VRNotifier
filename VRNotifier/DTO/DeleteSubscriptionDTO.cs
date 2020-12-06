namespace VRNotifier.DTO
{
    public class DeleteSubscriptionDTO
    {
        public string MediaName { get; set; }

        public DeleteSubscriptionDTO()
        {
        }

        public DeleteSubscriptionDTO(string mediaName)
        {
            MediaName = mediaName;
        }
    }
}