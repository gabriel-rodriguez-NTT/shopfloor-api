using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using ShopfloorAssistant.Core.Sql;

namespace ShopfloorAssistant.Core.Workflows
{
    public class SqlQueryExecutor : Executor<SqlQueryResult, SqlQueryResult>
    {
        private AIAgent _agent;
        private AgentThread _thread;
        private readonly ISqlQueryService _sqlQueryService;
        private IChatClient _chatClient;

        public SqlQueryExecutor(ISqlQueryService sqlQueryService) : base("SqlQueryExecutor")
        {
            _sqlQueryService = sqlQueryService;
        }

        public void Configure(string instructions, IChatClient chatClient)
        {
            _chatClient = chatClient;
            _agent = GetAgent(instructions);
            _thread = _agent.GetNewThread();
        }

        public AIAgent GetAgent(string instructions)
        {
            ChatClientAgentOptions agentOptions = new(
                instructions: instructions,
                name: "SqlExecuter",
                description: "An agent that executes SQL code.",
                tools: [.. _sqlQueryService.AsAITools()])
            {
                ChatOptions = new()
                {
                    //AllowBackgroundResponses = true,
                    //ToolMode = ChatToolMode.RequireAny
                }
            };

            return new ChatClientAgent(_chatClient, agentOptions);
        }

        public override async ValueTask<SqlQueryResult> HandleAsync(SqlQueryResult sqlQueryResult, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"[SQL Agent (Executor)]: Executing SQL query...", cancellationToken);
            Console.WriteLine($"[SQL Agent (Executor)]: Executing SQL query...", cancellationToken);
            //var response = await _agent.RunAsync(sqlQueryResult.Query, _thread, cancellationToken: cancellationToken);
            if (!string.IsNullOrEmpty(sqlQueryResult?.Query))
            {
                sqlQueryResult.QueryResult = _sqlQueryService.ExecuteSqlQuery(sqlQueryResult.Query);
            }

            await context.YieldOutputAsync($"[SQL Agent (Executor)]: SQL query executed", cancellationToken);
            Console.WriteLine($"[SQL Agent (Executor)]: SQL query executed", cancellationToken);

            //await context.AddEventAsync(new SqlWorkflowEvent(response.Text), cancellationToken);

            await context.SendMessageAsync(sqlQueryResult, cancellationToken: cancellationToken);
            return sqlQueryResult;
        }
    }
}
