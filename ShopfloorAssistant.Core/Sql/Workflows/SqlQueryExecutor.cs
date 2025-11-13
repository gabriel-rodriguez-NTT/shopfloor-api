using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ShopfloorAssistant.Core.Sql;

namespace ShopfloorAssistant.Core.Workflows
{
    public class SqlQueryExecutor : Executor<SqlQueryResult, SqlQueryResult>
    {
        private AIAgent _agent;
        private AgentThread _thread;
        private readonly ISqlQueryService _sqlQueryService;

        public SqlQueryExecutor(ISqlQueryService sqlQueryService) : base("SqlQueryExecutor")
        {
            _sqlQueryService = sqlQueryService;
        }

        public void Configure(string instructions, IChatClient chatClient)
        {
            Func<string, string> searchDelegate =
            _sqlQueryService.ExecuteSqlQuery;

            var aiFunction = AIFunctionFactory.Create(searchDelegate);

            ChatClientAgentOptions agentOptions = new(
                instructions: instructions, 
                name: "SqlExecuter",
                description: "An agent that executes SQL code.",
                tools: [aiFunction])
            {
            };

            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
        }

        public override async ValueTask<SqlQueryResult> HandleAsync(SqlQueryResult sqlQueryResult, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"[SQL Agent (Executor)]: Executing SQL query...", cancellationToken);
            Console.WriteLine($"[SQL Agent (Executor)]: Executing SQL query...", cancellationToken);
            var message = $"""
                User question: {sqlQueryResult.UserInput}
                Query: {sqlQueryResult.Query}
            """;

            var response = await _agent.RunAsync(message, _thread, cancellationToken: cancellationToken);
            sqlQueryResult.QueryResult = response.Text;

            await context.YieldOutputAsync($"[SQL Agent (Executor)]: SQL query executed", cancellationToken);
            Console.WriteLine($"[SQL Agent (Executor)]: SQL query executed", cancellationToken);

            //await context.AddEventAsync(new SqlWorkflowEvent(response.Text), cancellationToken);
            
            await context.SendMessageAsync(sqlQueryResult, cancellationToken: cancellationToken);
            return sqlQueryResult;
        }
    }
}
