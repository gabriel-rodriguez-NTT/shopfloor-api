using Azure.Search.Documents.Indexes.Models;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Responses;
using ShopfloorAssistant.Core.AiSearch;
using ShopfloorAssistant.Core.Email;
using ShopfloorAssistant.Core.Sql;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ShopfloorAssistant.Core.Workflows
{
    public class ToolExecutor : Executor<string, ToolResult>
    {
        private AIAgent _agent;
        private AgentThread _thread;
        private readonly ISqlQueryService _sqlQueryService;
        private readonly IAiSearchService _aiSearchService;
        private readonly IAgentPromptProvider _agentPromptProvider;
        private readonly IEmailService _emailService;
        private readonly ILogger<ToolExecutor> _logger;
        private readonly McpOptions _mcpOptions;

        public ToolExecutor(ISqlQueryService sqlQueryService,
            IAiSearchService aiSearchService,
            IAgentPromptProvider agentPromptProvider,
            ILogger<ToolExecutor> logger,
            IOptions<McpOptions> mcpOptions,
            IEmailService emailService) : base("ToolExecutor")
        {
            _sqlQueryService = sqlQueryService;
            _aiSearchService = aiSearchService;
            _agentPromptProvider = agentPromptProvider;
            _logger = logger;
            _mcpOptions = mcpOptions.Value;
            _emailService = emailService;
        }

        public async Task Configure(IChatClient chatClient)
        {
            Func<string, string, string> aiSearchFunction =
                _aiSearchService.ExecuteQuery;

            Func<string, string, string, Task<bool>> emailFunction = _emailService.SendEmailAsync;

            var emailAiFunction = AIFunctionFactory.Create(emailFunction);
            var aiSearchAiFunction = AIFunctionFactory.Create(aiSearchFunction);
            var instructions = await _agentPromptProvider.GetPromptAsync(AgentType.Tool);

            var mcpClient = await McpClient.CreateAsync(
               new HttpClientTransport(new()
               {
                   Name = _mcpOptions.Name,
                   Endpoint = new Uri(_mcpOptions.Endpoint),
                   TransportMode = HttpTransportMode.StreamableHttp
               }));

            var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

            IList<AITool> tools = [aiSearchAiFunction
                , .._sqlQueryService.AsAITools()
                , emailAiFunction
                , ..mcpTools.Cast<AITool>()
                ];
#pragma warning disable MEAI001 // Este tipo se incluye solo con fines de evaluación y está sujeto a cambios o a que se elimine en próximas actualizaciones. Suprima este diagnóstico para continuar.
            ChatClientAgentOptions agentOptions = new(
                instructions: instructions,
                name: "ToolExecuter",
                description: "An agent that executes none, one or multiple tools.",
                tools: tools)
            {
                ChatOptions = new()
                {
                    //ResponseFormat = ChatResponseFormat.ForJsonSchema<ToolResult>(),
                    AllowBackgroundResponses = true,
                    AllowMultipleToolCalls = true,
                    //ToolMode = ChatToolMode.RequireAny,
                    Tools = tools
                }
            };
#pragma warning restore MEAI001 // Este tipo se incluye solo con fines de evaluación y está sujeto a cambios o a que se elimine en próximas actualizaciones. Suprima este diagnóstico para continuar.

            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
        }

        public override async ValueTask<ToolResult> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            using (_logger.LogElapsed("[Tool Agent]: Executing Tool Agent"))
            {
                var response = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);
                await context.AddEventAsync(new SqlWorkflowEvent(response.Text), cancellationToken);

                await context.SendMessageAsync(response.Text, cancellationToken: cancellationToken);
                return new ToolResult();
            }
        }
    }

    /// <summary>
    /// Represents information about a person, including their name, age, and occupation, matched to the JSON schema used in the agent.
    /// </summary>
    [Description("Information about tool responses")]
    public class ToolResult
    {
        [JsonPropertyName("sqlResult")]
        public string? SqlResult { get; set; }

        [JsonPropertyName("aiSearchResult")]
        public string? AiSearchResult { get; set; }
    }
}
