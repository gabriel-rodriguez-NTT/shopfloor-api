using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using ShopfloorAssistant.Core.AiSearch;
using System.Text.Json;

namespace ShopfloorAssistant.Core.Workflows
{
    internal sealed class AISearchQueryExecutor : Executor
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _thread;
        private readonly ILogger _logger;
        private readonly IAiSearchService _aiSearchService;
        private readonly IChatClient _chatClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackExecutor"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the executor.</param>
        /// <param name="chatClient">The chat client to use for the AI agent.</param>
        public AISearchQueryExecutor(string instructions, string id, IChatClient chatClient, IAiSearchService aiSearchService, ILogger logger) : base(id)
        {
            _chatClient = chatClient;
            _aiSearchService = aiSearchService;
            _agent = GetAgent(instructions);
            _thread = _agent.GetNewThread();
            _logger = logger;
        }

        public AIAgent GetAgent(string instructions)
        {
            Func<string, string, string> searchDelegate =
            _aiSearchService.ExecuteQuery;

            var aiFunction = AIFunctionFactory.Create(searchDelegate);

            ChatClientAgentOptions agentOptions = new(
                instructions: instructions, tools: [aiFunction])
            {
            };

            return new ChatClientAgent(_chatClient, agentOptions);
        }

        protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder.AddHandler<string, AiSearchQueryResult>(HandleAsync);

        public async ValueTask<AiSearchQueryResult> HandleAsync(string query, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            using (_logger.LogElapsed("[AISearch Agent (Executor)]: Executing semantic search"))
            {
                var response = await _agent.RunAsync(query, _thread, cancellationToken: cancellationToken);

                var aiSearchResult = new AiSearchQueryResult()
                {
                    UserInput = query,
                    AiSearchResult = response.Text
                };

                await context.AddEventAsync(new AiSearchExecutedEvent(aiSearchResult), cancellationToken);
                return aiSearchResult;
            }
        }
    }

    public class AiSearchQueryResult
    {
        public string UserInput { get; set; }
        public string AiSearchResult { get; set; }
    }

    internal sealed class AiSearchExecutedEvent(AiSearchQueryResult result) : WorkflowEvent(result)
    {
        public override string ToString() => $"{result.AiSearchResult}";
    }

}
