using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
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
        private readonly OpenAIClient _chatClient;
        private readonly ILogger _logger;
        private McpClient _mcpClient;
        private IList<McpClientTool>? _mcpTools;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackExecutor"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the executor.</param>
        /// <param name="chatClient">The chat client to use for the AI agent.</param>
        public McpExecutor(OpenAIClient chatClient, McpOptions mcpOptions, ILogger logger) : base("McpExecuter")
        {
            _mcpOptions = mcpOptions;
            _chatClient = chatClient;
            _logger = logger;
        }

        public async Task Configure()
        {
            _agent = await GetAgent();
            _thread = _agent.GetNewThread();
        }

        public async Task<AIAgent> GetAgent()
        {
            _mcpClient = await McpClient.CreateAsync(
            new HttpClientTransport(new()
            {
                Name = _mcpOptions.Name,
                Endpoint = new Uri(_mcpOptions.Endpoint),
                TransportMode = HttpTransportMode.StreamableHttp
            }));

            _mcpTools = await _mcpClient.ListToolsAsync().ConfigureAwait(false);

            return _chatClient
                .GetChatClient(_mcpOptions.ModelName)
                .CreateAIAgent(instructions: _mcpOptions.Instructions, tools: [.. _mcpTools.Cast<AITool>()]);
        }

        protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder.AddHandler<string, string>(HandleAsync);

        public async ValueTask<string> HandleAsync(string query, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            using (_logger.LogElapsed("[MCP Agent]: Executing Jira agent"))
            {
                var response = await _agent.RunAsync(query, cancellationToken: cancellationToken);

                await context.AddEventAsync(new WorkflowEvent(response.Text), cancellationToken);
                await context.SendMessageAsync(response.Text, cancellationToken: cancellationToken);
                return response.Text;
            }
        }
    }
}
