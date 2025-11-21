using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenAI.Chat;
using OpenAI.Responses;
using ShopfloorAssistant.Core.Email;
using ShopfloorAssistant.Core.Workflows;
using System.Threading;

namespace ShopfloorAssistant.Core
{
    internal sealed class ConcurrentAggregationExecutor : Executor
    {
        private readonly List<object> _messages = [];
        private AiSearchQueryResult aiSearchQueryResult;
        private SqlQueryResult sqlSearchQueryResult;
        private object mcpResponse;
        private AIAgent _agent;
        private AgentThread _thread;
        private McpOptions _mcpOptions;
        private IEmailService _emailService;
        private readonly ILogger _logger;

        public ConcurrentAggregationExecutor(McpOptions mcpOptions, IEmailService emailService, ILogger logger) : base("ConcurrentAggregationExecutor")
        {
            _mcpOptions = mcpOptions;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Configure(string instructions, IChatClient chatClient)
        {
            Func<string, string, string, Task<string>> searchDelegate = _emailService.SendEmailAsync;
            var aiFunction = AIFunctionFactory.Create(searchDelegate);

            ChatClientAgentOptions agentOptions = new(
                instructions: instructions,
                name: "Anylizer",
                description: "An agent that analyzes SQL and AI Search results.",
                tools: [aiFunction])
            {
            };

            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
        }

        protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder.AddHandler<AiSearchQueryResult>(HandleAsync)
            .AddHandler<SqlQueryResult>(HandleAsync)
            .AddHandler<string>(HandleAsync)
            ;

        /// <summary>
        /// Handles incoming messages from the agents and aggregates their responses.
        /// </summary>
        /// <param name="message">The message from the agent</param>
        /// <param name="context">Workflow context for accessing workflow services and adding events</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.
        /// The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async ValueTask HandleAsync(object message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            mcpResponse = message;

            await context.YieldOutputAsync(message, cancellationToken);
            await GenerateAnalysis(context);
        }

        /// <summary>
        /// Handles incoming messages from the agents and aggregates their responses.
        /// </summary>
        /// <param name="message">The message from the agent</param>
        /// <param name="context">Workflow context for accessing workflow services and adding events</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.
        /// The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async ValueTask HandleAsync(AiSearchQueryResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            aiSearchQueryResult = message;

            await context.YieldOutputAsync(message, cancellationToken);
            await GenerateAnalysis(context);
        }

        /// <summary>
        /// Handles incoming messages from the agents and aggregates their responses.
        /// </summary>
        /// <param name="message">The message from the agent</param>
        /// <param name="context">Workflow context for accessing workflow services and adding events</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests.
        /// The default is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async ValueTask HandleAsync(SqlQueryResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            this._messages.Add(message);
            sqlSearchQueryResult = message;
            await context.YieldOutputAsync(message, cancellationToken);
            await GenerateAnalysis(context);
        }

        private async Task GenerateAnalysis(IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            if (sqlSearchQueryResult is null || aiSearchQueryResult is null || mcpResponse is null)
            {
                return;
            }
            using (_logger.LogElapsed("[[Anylizer Agent]: Anylizing SQL and AI Search"))
            {
                var message = $"""
                User question: {sqlSearchQueryResult.UserInput}
                QueryResult: {sqlSearchQueryResult.QueryResult}
                AI Search Result: {aiSearchQueryResult.AiSearchResult}
                Jira result: {mcpResponse}
                """;

                var response = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);

                await context.AddEventAsync(new SqlWorkflowEvent(response.Text), cancellationToken);

                await context.SendMessageAsync(response.Text, cancellationToken: cancellationToken);
            }
        }
    }
}
