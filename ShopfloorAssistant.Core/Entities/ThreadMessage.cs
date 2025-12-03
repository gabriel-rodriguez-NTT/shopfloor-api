using System;
using Microsoft.Extensions.AI;

namespace ShopfloorAssistant.Core.Entities
{
    public class ThreadMessage
    {
        public ThreadMessage()
        {
            
        }
        public Guid ThreadId { get; set; } // Foreign Key
        public string Id { get; set; } // Primary Key
        public required string Message { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public string Role { get; set; }
        public virtual Thread Thread { get; set; }
        public string? ToolCallId { get; set; }
        public int Order { get; set; }
        public virtual ICollection<ThreadToolCall> ToolCalls { get; set; } = new List<ThreadToolCall>();
    }
}
