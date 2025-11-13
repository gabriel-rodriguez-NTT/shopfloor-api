using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;
using OpenAI.Chat;
using ShopfloorAssistant.Core.AiSearch;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ShopfloorAssistant.Core
{
    internal sealed class McpExecutor : Executor
    {
        private AIAgent _agent;
        private AgentThread _thread;
        private readonly McpOptions _mcpOptions;
        private readonly AzureOpenAIClient _chatClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackExecutor"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the executor.</param>
        /// <param name="chatClient">The chat client to use for the AI agent.</param>
        public McpExecutor(AzureOpenAIClient chatClient, McpOptions mcpOptions) : base("McpExecuter")
        {
            _mcpOptions = mcpOptions;
            _chatClient = chatClient;
        }

        public async Task Configure()
        {
            await using var mcpClient = await McpClient.CreateAsync(
            new HttpClientTransport(new()
            {
                Name = _mcpOptions.Name,
                Endpoint = new Uri(_mcpOptions.Endpoint),
                TransportMode = HttpTransportMode.StreamableHttp
            }));

            var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

            //ChatClientAgentOptions agentOptions = new(
            //    instructions: _mcpOptions.Instructions, tools: [..mcpTools.Cast<AITool>()])
            //{
            //};

            //_agent = new ChatClientAgent(_chatClient, agentOptions);
            _agent = _chatClient
                .GetChatClient(_mcpOptions.ModelName)
                .CreateAIAgent(instructions: _mcpOptions.Instructions, tools: [.. mcpTools.Cast<AITool>()]);
            _thread = _agent.GetNewThread();
        }

        protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder.AddHandler<string, string>(HandleAsync);

        public async ValueTask<string> HandleAsync(string query, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"[MCP Agent]: Executing Jira agent...", cancellationToken);
            Console.WriteLine($"[MCP Agent]: Executing Jira agent...", cancellationToken);
            
            var response = await _agent.RunAsync(query, cancellationToken: cancellationToken);
            
            await context.YieldOutputAsync($"[MCP Agent]: Jira agent executed...", cancellationToken);
            Console.WriteLine($"[MCP Agent]: Jira agent executed...", cancellationToken);

            await context.AddEventAsync(new WorkflowEvent(response.Text), cancellationToken);

            await context.SendMessageAsync(response.Text, cancellationToken: cancellationToken);
            return response.Text;
        }
    }
}
