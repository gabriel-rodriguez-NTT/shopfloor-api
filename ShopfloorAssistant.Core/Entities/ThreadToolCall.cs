namespace ShopfloorAssistant.Core.Entities
{
    public class ThreadToolCall
    {
        public Guid Id { get; set; }
        public string CallId { get; set; } = default!;
        public required string Name { get; set; }  // e.g. "ExecuteQuery"
        public string? Result { get; set; }  // e.g. "ExecuteQuery"

        // Arguments generales, basados en tu ejemplo, pero agnósticos
        public IDictionary<string, object>? Arguments { get; set; }

        // Relación con el mensaje que originó el tool call
        public string ThreadMessageId { get; set; }
        public virtual ThreadMessage ThreadMessage { get; set; }
    }
}
