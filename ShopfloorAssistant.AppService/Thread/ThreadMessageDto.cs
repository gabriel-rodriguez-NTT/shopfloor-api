namespace ShopfloorAssistant.AppService
{
    public class ThreadMessageDto
    {
        public string Id { get; set; } // Primary Key
        public Guid ThreadId { get; set; } // Foreign Key
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset? CreateAt { get; set; }
        public string Role { get; set; }
        public string? ToolCallId { get; set; }
    }
}
