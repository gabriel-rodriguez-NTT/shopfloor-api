using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ShopfloorAssistant.Core.Sql;

namespace ShopfloorAssistant.Core.Workflows
{
    public class SqlQueryAnylizer : Executor<SqlQueryResult, string>
    {
        private AIAgent _agent;
        private AgentThread _thread;

        public SqlQueryAnylizer(string instructions, string id, IChatClient chatClient) : base(id)
        {
            ChatClientAgentOptions agentOptions = new(
                instructions: instructions,
                name: "SqlAnylizer",
                description: "An agent that analyzes SQL results.")
            {
            };


            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
        }

        public override async ValueTask<string> HandleAsync(SqlQueryResult sqlQueryResult, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"[SQL Agent (Anylizer)]: Anylizing SQL results \n\t{sqlQueryResult.QueryResult}...", cancellationToken);
            var message = $"""
                User question: {sqlQueryResult.UserInput}
                QueryResult: {sqlQueryResult.QueryResult}
            """;

            var response = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);
            sqlQueryResult.QueryResult = response.Text;

            await context.YieldOutputAsync($"[SQL Agent (Anylizer)]: SQL query anylized", cancellationToken);

            await context.AddEventAsync(new SqlWorkflowEvent(response.Text), cancellationToken);

            await context.SendMessageAsync(sqlQueryResult, cancellationToken: cancellationToken);
            return response.Text;
        }
    }
}
