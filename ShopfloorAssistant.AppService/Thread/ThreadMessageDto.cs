using System.Text.Json.Serialization;

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
        public ThreadCallDto[]? ToolCalls { get; set; }
    }

    public class ThreadCallDto
    {
        public string Id { get; set; } // Primary Key
        public string Type { get; set; } = "function";
        public ThreadFunctionCallDto Function { get; set; } = new();
    }

    public class ThreadFunctionCallDto
    {
        public string Name { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
    }
}
