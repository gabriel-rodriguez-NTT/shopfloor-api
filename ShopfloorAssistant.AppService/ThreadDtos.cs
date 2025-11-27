using System;
using Microsoft.Extensions.AI;

namespace ShopfloorAssistant.AppService
{
    public class ThreadDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public List<ThreadMessageDto> Messages { get; set; } = new();
    }

    public class ThreadMessageDto
    {
        public string Id { get; set; } // Primary Key
        public Guid ThreadId { get; set; } // Foreign Key
        public string Content { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public string Role { get; set; }
    }
}
