using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using ShopfloorAssistant.Core.Sql;

namespace ShopfloorAssistant.Core.Workflows
{
    public class SqlQueryExecutor : Executor<SqlQueryResult, SqlQueryResult>
    {
        private AIAgent _agent;
        private AgentThread _thread;
        private readonly ISqlQueryService _sqlQueryService;
        private readonly ILogger<SqlQueryExecutor> _logger;
        private IChatClient _chatClient;

        public SqlQueryExecutor(ISqlQueryService sqlQueryService, ILogger<SqlQueryExecutor> logger) : base("SqlQueryExecutor")
        {
            _sqlQueryService = sqlQueryService;
            _logger = logger;
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
            using (_logger.LogElapsed("[SQL Agent (Executor)]: Executing query"))
            {
                if (!string.IsNullOrEmpty(sqlQueryResult?.Query))
                {
                    sqlQueryResult.QueryResult = _sqlQueryService.ExecuteSqlQuery(sqlQueryResult.Query);
                }

                await context.SendMessageAsync(sqlQueryResult, cancellationToken: cancellationToken);
                return sqlQueryResult;
            }
        }
    }
}
