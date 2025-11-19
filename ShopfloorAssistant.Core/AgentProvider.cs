using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using OpenAI;
using ShopfloorAssistant.Core.AiSearch;
using ShopfloorAssistant.Core.Email;
using ShopfloorAssistant.Core.Workflows;
using System.ClientModel;

namespace ShopfloorAssistant.Core.AgentsConfig
{
    public class AgentProvider : IAgentProvider
    {
        private readonly IAgentPromptProvider _promptProvider;
        private readonly OpenAIClient _openAIClient;
        private readonly SqlQueryExecutor _sqlQueryExecutor;
        private readonly ToolExecutor _toolExecutor;
        private readonly IAiSearchService _aiSearchService;
        private readonly IEmailService _emailService;
        private readonly OpenAiOptions _openAiOptions;
        private readonly McpOptions _mcpOptions;

        public AgentProvider(
            IOptions<OpenAiOptions> openAiOptions,
            IOptions<McpOptions> mcpOptions,
            IAgentPromptProvider promptProvider,
            SqlQueryExecutor sqlQueryExecutor,
            ToolExecutor toolExecutor,
            IAiSearchService aiSearchService,
            IEmailService emailService
        )
        {
            _promptProvider = promptProvider;
            _emailService = emailService;
            _openAiOptions = openAiOptions.Value ?? throw new ArgumentNullException(nameof(openAiOptions));
            _mcpOptions = mcpOptions.Value ?? throw new ArgumentNullException(nameof(mcpOptions));
            var endpoint = _openAiOptions.Endpoint;

            var credential = new ApiKeyCredential(_openAiOptions.AgentModelApiKey);
            _openAIClient = new AzureOpenAIClient(new Uri(endpoint), credential);

            //var options = new OpenAIClientOptions
            //{
            //    Endpoint = new Uri(endpoint),
            //};
            //_openAIClient = new OpenAIClient(credential, options);
            _sqlQueryExecutor = sqlQueryExecutor;
            _aiSearchService = aiSearchService;
            _toolExecutor = toolExecutor;
        }

        public async Task<Workflow> GetAiSearchWorkflow()
        {
            Console.WriteLine($"Creating AI Search Workflow...");
            var client = _openAIClient
                    .GetChatClient(_openAiOptions.AgentsModel)
                    .AsIChatClient();
            var aiSearchPromptBuilder = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryBuilder);
            var aiSearchPromptExecutor = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryExecutor);
            var aiSearchPromptAnalyzer = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryAnalyzer);

            //var aiSearchQueryBuilder = new AiSearchQueryBuilder(aiSearchPromptBuilder, "AiSearchQueryBuilder", client);
            var aiSearchQueryExecutor = new AISearchQueryExecutor(aiSearchPromptExecutor, "AISearchQueryExecutor", client, _aiSearchService);
            var aiSearchQueryAnalizer = new AISearchQueryAnalyzer(aiSearchPromptAnalyzer, "AISearchQueryAnalyzer", client);

            var workflow = new WorkflowBuilder(aiSearchQueryExecutor)
            .AddEdge(aiSearchQueryExecutor, aiSearchQueryAnalizer)
            .WithOutputFrom(aiSearchQueryAnalizer)
            .Build();
            Console.WriteLine($"AI Search Workflow created...");
            return workflow;
        }

        public async Task<Workflow> GetConcurrentWorkflow()
        {
            var agents = new Dictionary<AgentType, object>();
            var client = _openAIClient
                    .GetChatClient(_openAiOptions.AgentsModel)
                    .AsIChatClient();
            var aiSearchPromptExecutor = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryExecutor);
            var aiSearchPromptAnalyzer = await _promptProvider.GetPromptAsync(AgentType.AiSearchQueryAnalyzer);

            var sqlPromptBuilder = await _promptProvider.GetPromptAsync(AgentType.SqlBuilder);
            var sqlPromptExecutor = await _promptProvider.GetPromptAsync(AgentType.SqlExecuter);
            var sqlPromptAnylizer = await _promptProvider.GetPromptAsync(AgentType.SqlAnylizer);
            var promptAnylizer = await _promptProvider.GetPromptAsync(AgentType.Anylizer);

            var aiSearchQueryExecutor = new AISearchQueryExecutor(aiSearchPromptExecutor, "AISearchQueryExecutor", client, _aiSearchService);
            var aiSearchQueryAnalizer = new AISearchQueryAnalyzer(aiSearchPromptAnalyzer, "AISearchQueryAnalyzer", client);

            var sqlQueryBuilder = new SqlQueryBuilder(sqlPromptBuilder, "SQLQueryBuilder", client);
            var sqlQueryAnylizer = new SqlQueryAnylizer(sqlPromptAnylizer, "SQLQueryAnylizer", client);
            _sqlQueryExecutor.Configure(sqlPromptExecutor, client);

            var concurrentStartExecutor = new ConcurrentStartExecutor();
            var aggregationExecutor = new ConcurrentAggregationExecutor(_mcpOptions, _emailService);
            await aggregationExecutor.Configure(promptAnylizer, client);

            var mcpAgent = new McpExecutor(_openAIClient, _mcpOptions);
            await mcpAgent.Configure();

            var workflow = new WorkflowBuilder(concurrentStartExecutor)
            .AddFanOutEdge(concurrentStartExecutor, targets: [aiSearchQueryExecutor, sqlQueryBuilder, mcpAgent])
            .AddFanInEdge([_sqlQueryExecutor, aiSearchQueryExecutor, mcpAgent], aggregationExecutor)
            .AddEdge(sqlQueryBuilder, _sqlQueryExecutor)
            .WithOutputFrom(aggregationExecutor)
            .Build();
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
            //await using var mcpClient = await McpClient.CreateAsync(
            //    new StdioClientTransport(new()
            //    {
            //        Name = "Atlassian MCP",
            //        Command = "npx",
            //        Arguments = [
            //            //"http://127.0.0.1:8096/servers/jira/sse"
            //            "-y",
            //            "--verbose",
            //            "mcp-remote",
            //            "https://mcp.atlassian.com/v1/sse"
            //        ]
            //    }),
            //    new McpClientOptions()
            //    {
            //        ClientInfo = new ModelContextProtocol.Protocol.Implementation()
            //        {
            //            Name = ".Net APP Shopfloor",
            //            Version = "1.0.0.0"
            //        }
            //    });

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

            await using var mcpClient = await McpClient.CreateAsync(
                new HttpClientTransport(new()
                {
                    Name = _mcpOptions.Name,
                    Endpoint = new Uri(_mcpOptions.Endpoint),
                    TransportMode = HttpTransportMode.StreamableHttp
                }));

            var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
            
            AIAgent agent = _openAIClient
             .GetChatClient(_mcpOptions.ModelName ?? _openAiOptions.AgentsModel)
             .CreateAIAgent(instructions: _mcpOptions.Instructions, tools: [.. mcpTools.Cast<AITool>()]);
            var response = await agent.RunAsync(input);
            return response.Text;
        }
        
        public async Task<Workflow> GetToolWorkflow()
        {
            var client = _openAIClient
                    .GetChatClient(_openAiOptions.AgentsModel)
                    .AsIChatClient();
            await _toolExecutor.Configure(client);
            var aggregationExecutor = new ConcurrentAggregationExecutor(_mcpOptions, _emailService);
            var workflow = new WorkflowBuilder(_toolExecutor)
                //.AddEdge(_toolExecutor, aggregationExecutor)
                .WithOutputFrom(_toolExecutor)
                .Build();
            return workflow;
        }
    }
}
