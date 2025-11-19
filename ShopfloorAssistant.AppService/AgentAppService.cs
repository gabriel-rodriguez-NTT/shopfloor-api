using Microsoft.Agents.AI.Workflows;
using ShopfloorAssistant.Core.AgentsConfig;
using ShopfloorAssistant.Core.Workflows;

namespace ShopfloorAssistant.AppService
{
    public class AgentAppService : IAgentAppService
    {
        private readonly IAgentProvider _agentConfigurator;

        public AgentAppService(IAgentProvider agentConfigurator)
        {
            _agentConfigurator = agentConfigurator ?? throw new ArgumentNullException(nameof(agentConfigurator));
        }

        public async Task<WorkflowEvent> RunWorkflowAsync(string workflowType, string message)
        {
            if (string.IsNullOrWhiteSpace(workflowType))
                throw new ArgumentException("Workflow type is required.", nameof(workflowType));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message input is required.", nameof(message));

            Workflow workflow = workflowType.ToLowerInvariant() switch
            {
                "ai-search" => await _agentConfigurator.GetAiSearchWorkflow(),
                "sql-search" => await _agentConfigurator.GetSqlWorkflow(),
                "concurrent" => await _agentConfigurator.GetConcurrentWorkflow(),
                "tool" => await _agentConfigurator.GetToolWorkflow(),
                _ => throw new InvalidOperationException($"Invalid workflow type '{workflowType}'.")
            };

            Console.WriteLine($"Running Workflow...");
            
            await using StreamingRun run = await InProcessExecution.StreamAsync(workflow, input: message);
            await foreach (WorkflowEvent evt in run.WatchStreamAsync())
            {
                switch (evt)
                {
                    case WorkflowOutputEvent outputEvent:
                        //Console.WriteLine($"[Workflow Output]: {outputEvent}");
                        break;

                    case AiSearchEvent aiSearchEvent:
                        Console.WriteLine($"[AI Search Event]: {aiSearchEvent}");
                        return aiSearchEvent;

                    case SqlWorkflowEvent sqlWorkflowEvent:
                        Console.WriteLine($"[SQL Workflow Event]: {sqlWorkflowEvent}");
                        return sqlWorkflowEvent;
                }
            }

            return new WorkflowEvent("Workflow completed without output events.");
        }


        public async Task<string> RunMcpTest(string message)
        {
            var agent = await _agentConfigurator.McpTest(message);
            return agent;
        }
    }
}
