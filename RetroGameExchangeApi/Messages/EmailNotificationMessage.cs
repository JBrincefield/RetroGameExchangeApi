namespace RetroGameExchangeApi.Messages
{
    public class EmailNotificationMessage
    {
        public string ToEmail { get; set; } = "";
        public string ToName { get; set; } = "";
        public string Subject { get; set; } = "";
        public string Body { get; set; } = "";
        public string EventType { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}