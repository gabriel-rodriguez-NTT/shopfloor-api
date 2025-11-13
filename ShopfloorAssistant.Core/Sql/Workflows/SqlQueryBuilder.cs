using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace ShopfloorAssistant.Core.Workflows
{
    internal sealed class SqlQueryBuilder : Executor
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _thread;

        /// <summary>
        /// Initializes a new instance of the <see cref="SloganWriterExecutor"/> class.
        /// </summary>
        /// <param name="id">A unique identifier for the executor.</param>
        /// <param name="chatClient">The chat client to use for the AI agent.</param>
        public SqlQueryBuilder(string instructions, string id, IChatClient chatClient) : base(id)
        {
            ChatClientAgentOptions agentOptions = new(instructions: instructions)
            {
            };

            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
        }

        protected override RouteBuilder ConfigureRoutes(RouteBuilder routeBuilder) =>
        routeBuilder.AddHandler<string, SqlQueryResult>(this.HandleAsync);

        public async ValueTask<SqlQueryResult> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"[SQL Agent (Builder)]: Generating query...", cancellationToken);
            Console.WriteLine($"[SQL Agent (Builder)]: Generating query...", cancellationToken);
            var result = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);

            var query = result.Text ?? throw new InvalidOperationException("Failed to deserialize slogan result.");
            await context.YieldOutputAsync($"[SQL Agent (Builder)]: Query generated \n\t{query}", cancellationToken);
            Console.WriteLine($"[SQL Agent (Builder)]: Query generated \n\t{query}");

            var response = new SqlQueryResult()
            {
                UserInput = message,
                Query = query
            };
            await context.AddEventAsync(new SqlQueryBuildEvent(response), cancellationToken);
            return response;
        }
    }

    public class SqlQueryResult
    {
        public string UserInput { get; set; }
        public string Query { get; set; }
        public string QueryResult { get; set; }
    }

    internal sealed class SqlQueryBuildEvent(SqlQueryResult result) : WorkflowEvent(result)
    {
        public override string ToString() => $"{result.Query}\n{result.QueryResult}";
    }
}
