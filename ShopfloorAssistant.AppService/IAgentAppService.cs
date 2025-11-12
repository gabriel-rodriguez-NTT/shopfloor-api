using Microsoft.Agents.AI.Workflows;

namespace ShopfloorAssistant.AppService
{
    public interface IAgentAppService
    {
        Task<WorkflowEvent> RunWorkflowAsync(string workflowType, string message);
        Task<string> RunMcpTest(string message);
        //Task McpSimpleAsync();
    }
}
