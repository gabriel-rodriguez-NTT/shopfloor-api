using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ShopfloorAssistant.Core.Sql;
using Microsoft.Extensions.Logging;

namespace ShopfloorAssistant.Core.Workflows
{
    public class SqlQueryAnylizer : Executor<SqlQueryResult, string>
    {
        private AIAgent _agent;
        private AgentThread _thread;
        private ILogger _logger;

        public SqlQueryAnylizer(string instructions, string id, IChatClient chatClient, ILogger logger) : base(id)
        {
            ChatClientAgentOptions agentOptions = new(
                instructions: instructions,
                name: "SqlAnylizer",
                description: "An agent that analyzes SQL results.")
            {
            };


            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
            _logger = logger;
        }

        public override async ValueTask<string> HandleAsync(SqlQueryResult sqlQueryResult, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            using (_logger.LogElapsed("[SQL Agent (Anylizer)]: Anylicing query results"))
            {
                var message = $"""
                User question: {sqlQueryResult.UserInput}
                QueryResult: {sqlQueryResult.QueryResult}
                """;

                var response = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);
                sqlQueryResult.QueryResult = response.Text;

                await context.AddEventAsync(new SqlWorkflowEvent(response.Text), cancellationToken);
                await context.SendMessageAsync(sqlQueryResult, cancellationToken: cancellationToken);
                return response.Text;
            }
        }
    }
}
