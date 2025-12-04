using System;

namespace ShopfloorAssistant.AppService
{
    public class PromptSuggestionDto : AuditableDto
    {
        public Guid Id { get; set; }
        public string Prompt { get; set; } = default!;
        public string? Description { get; set; }
        public double? Score { get; set; }

        // Serialized JSON representation of Metadata (maps to IDictionary<string, object> in the entity)
        public string? Metadata { get; set; }
    }

    public class PromptSuggestionCreateDto
    {
        public required string Prompt { get; set; }
        public string? Description { get; set; }
        public double? Score { get; set; }
        public string? Metadata { get; set; }
    }

    public class PromptSuggestionUpdateDto
    {
        public string? Prompt { get; set; }
        public string? Description { get; set; }
        public double? Score { get; set; }
        public string? Metadata { get; set; }
    }
}
