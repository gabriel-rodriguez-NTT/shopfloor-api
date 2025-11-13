using Microsoft.Agents.AI.Workflows;

namespace ShopfloorAssistant.Core.AgentsConfig
{
    public interface IAgentProvider
    {
        Task<Workflow> GetAiSearchWorkflow();
        Task<Workflow> GetSqlWorkflow();
        Task<string> McpTest(string input);
        Task<Workflow> GetConcurrentWorkflow();
    }
}
