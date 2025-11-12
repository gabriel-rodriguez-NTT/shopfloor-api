using Microsoft.Agents.AI.Workflows;
using System.Text.Json;

namespace ShopfloorAssistant.Core.Workflows
{
    public class SqlWorkflowEvent(string feedbackResult) : WorkflowEvent(feedbackResult)
    {
        private readonly JsonSerializerOptions _options = new() { WriteIndented = true };
        public override string ToString() => $"SQL Query: {feedbackResult}";
    }

    public class AiSearchEvent(string feedbackResult) : WorkflowEvent(feedbackResult)
    {
        public override string ToString() => $"{feedbackResult}";
    }
}
