using System;
using System.Collections.Generic;

namespace ShopfloorAssistant.Core.Entities
{
    public class PromptSuggestion : AuditableEntity
    {
        public Guid Id { get; set; }

        // The prompt text suggested
        public required string Prompt { get; set; }

        // Optional human readable description or context
        public string? Description { get; set; }

        // Optional relevance/score for ordering suggestions
        public double? Score { get; set; }

        // Additional metadata that can store arbitrary values (will be serialized to JSON in EF configuration)
        public IDictionary<string, object>? Metadata { get; set; }
    }
}
