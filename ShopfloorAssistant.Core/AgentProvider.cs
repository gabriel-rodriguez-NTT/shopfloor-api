using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using OpenAI;
using ShopfloorAssistant.Core.AiSearch;
using ShopfloorAssistant.Core.Workflows;
using System.ClientModel;

namespace ShopfloorAssistant.Core.AgentsConfig
{
    public class AgentProvider : IAgentProvider
    {
        private readonly IAgentPromptProvider _promptProvider;
        private readonly AzureOpenAIClient _openAIClient;
        private readonly SqlQueryExecutor _sqlQueryExecutor;
        private readonly IAiSearchService _aiSearchService;
        private readonly OpenAiOptions _openAiOptions;

        public AgentProvider(
            IOptions<OpenAiOptions> openAiOptions,
            IAgentPromptProvider promptProvider,
            SqlQueryExecutor sqlQueryExecutor,
            IAiSearchService aiSearchService
        )
        {
            _promptProvider = promptProvider;
            _openAiOptions = openAiOptions.Value ?? throw new ArgumentNullException(nameof(openAiOptions));
            var endpoint = _openAiOptions.Endpoint;
            var credential = new ApiKeyCredential(_openAiOptions.AgentModelApiKey);
            _openAIClient = new AzureOpenAIClient(new Uri(endpoint), credential);
            _sqlQueryExecutor = sqlQueryExecutor;
            _aiSearchService = aiSearchService;
        }

        public async Task<Workflow> GetAiSearchWorkflow()
        {
            Console.WriteLine($"Creating AI Search Workflow...");
            var agents = new Dictionary<AgentType, object>();
            var client = _openAIClient
                    .GetChatClient(_openAiOptions.AgentsModel)
                    .AsIChatClient();
            var aiSearchPromptBuilder = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryBuilder);
            var aiSearchPromptExecutor = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryExecutor);
            var aiSearchPromptAnalyzer = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryAnalyzer);

            //var aiSearchQueryBuilder = new AiSearchQueryBuilder(aiSearchPromptBuilder, "AiSearchQueryBuilder", client);
            var aiSearchQueryExecutor = new AISearchQueryExecutor(aiSearchPromptExecutor, "AISearchQueryExecutor", client, _aiSearchService);
            var aiSearchQueryAnalizer = new AISearchQueryAnalyzer(aiSearchPromptAnalyzer, "AISearchQueryAnalyzer", client);

            //var concurrentStartExecutor = new ConcurrentStartExecutor();
            //var aggregationExecutor = new ConcurrentAggregationExecutor();

            //var workflow = new WorkflowBuilder(concurrentStartExecutor)
            //.AddFanOutEdge(concurrentStartExecutor, targets: [aiSearchQueryBuilder, sqlQueryBuilder])
            //.AddFanInEdge(aggregationExecutor, sources: [aiSearchQueryExecutor, sqlQueryBuilder])
            //.AddEdge(aiSearchQueryBuilder, aiSearchQueryExecutor)
            //.WithOutputFrom(aggregationExecutor)
            //.Build();

            var workflow = new WorkflowBuilder(aiSearchQueryExecutor)
            .AddEdge(aiSearchQueryExecutor, aiSearchQueryAnalizer)
            .WithOutputFrom(aiSearchQueryAnalizer)
            .Build();
            Console.WriteLine($"AI Search Workflow created...");
            return workflow;
        }


        public async Task<Workflow> GetSqlWorkflow()
        {
            var agents = new Dictionary<AgentType, object>();
            var client = _openAIClient
                    .GetChatClient(_openAiOptions.AgentsModel)
                    .AsIChatClient();
            var sqlPromptBuilder = await _promptProvider.GetPromptAsync(AgentType.SqlBuilder);
            var sqlPromptExecutor = await _promptProvider.GetPromptAsync(AgentType.SqlExecuter);
            var sqlPromptAnylizer = await _promptProvider.GetPromptAsync(AgentType.SqlAnylizer);

            var sqlQueryBuilder = new SqlQueryBuilder(sqlPromptBuilder, "SQLQueryBuilder", client);
            var sqlQueryAnylizer = new SqlQueryAnylizer(sqlPromptAnylizer, "SQLQueryAnylizer", client);
            _sqlQueryExecutor.Configure(sqlPromptExecutor, client);

            var workflow = new WorkflowBuilder(sqlQueryBuilder)
            .AddEdge(sqlQueryBuilder, _sqlQueryExecutor)
            .AddEdge(_sqlQueryExecutor, sqlQueryAnylizer)
            .WithOutputFrom(sqlQueryAnylizer)
            .Build();

            return workflow;
        }

        public async Task<string> McpTest(string input)
        {
            await using var mcpClient = await McpClient.CreateAsync(
                new StdioClientTransport(new()
                {
                    Name = "Atlassian MCP",
                    Command = "mcp-proxy",
                    Arguments = [
                        //"http://127.0.0.1:8096/servers/jira/sse"
                        "-y",
                        "--verbose",
                        "mcp-remote",
                        "https://mcp.atlassian.com/v1/sse"
                    ]
                }),
                new McpClientOptions()
                {
                    ClientInfo = new ModelContextProtocol.Protocol.Implementation()
                    {
                        Name = ".Net APP Shopfloor",
                        Version = "1.0.0.0"
                    }
                });

            //await using var mcpClient = await McpClient.CreateAsync(
            //    new HttpClientTransport(new()
            //    {
            //        Name = "Github",
            //        Endpoint = new Uri("https://mcp-api-shopfloor.azure-api.net/github/mcp"),
            //        TransportMode = HttpTransportMode.AutoDetect,
            //         AdditionalHeaders = new Dictionary<string, string>()
            //         {
            //             { "Authorization",  "Bearer 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"}
            //         }
            //    }));
            //await using var mcpClient = await McpClient.CreateAsync(
            //    new HttpClientTransport(new()
            //    {
            //        Name = "Jira",
            //        Endpoint = new Uri("https://jira-mcp.kindgrass-76bbda58.westeurope.azurecontainerapps.io/mcp"),
            //        TransportMode = HttpTransportMode.StreamableHttp
            //    }));

            var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
            AIAgent agent = _openAIClient
             .GetChatClient("gpt-4o-mini")
             .CreateAIAgent(instructions: "You answer questions related to Github MCP only.", tools: [.. mcpTools.Cast<AITool>()]);
            var response = await agent.RunAsync(input);
            return response.Text;
        }
    }
}
