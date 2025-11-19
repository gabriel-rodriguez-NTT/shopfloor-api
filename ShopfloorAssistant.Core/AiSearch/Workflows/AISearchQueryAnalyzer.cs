using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;

namespace ShopfloorAssistant.Core.Workflows
{
    internal sealed class AISearchQueryAnalyzer : Executor<AiSearchQueryResult, string>
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _thread;
        private readonly ILogger _logger;
        private readonly IChatClient _chatClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="id"></param>
        /// <param name="chatClient"></param>
        public AISearchQueryAnalyzer(string instructions, string id, IChatClient chatClient, ILogger logger) : base(id)
        {
            _chatClient = chatClient;
            _agent = GetAgent(instructions);
            _thread = _agent.GetNewThread();
            _logger = logger;
        }

        public AIAgent GetAgent(string instructions)
        {
            ChatClientAgentOptions agentOptions = new(
                instructions: instructions)
            {

            };

            return new ChatClientAgent(_chatClient, agentOptions);
        }

        public override async ValueTask<string> HandleAsync(AiSearchQueryResult result, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            using (_logger.LogElapsed("[AISearch Agent (Analyzer)]: Analyzing AI Search results"))
            {
                var input = $"""
                User question: {result.UserInput}
                AiSearchResult: {result.AiSearchResult}
                """;
                var response = await _agent.RunAsync(input, _thread, cancellationToken: cancellationToken);
                await context.AddEventAsync(new AiSearchEvent(response.Text), cancellationToken);
                await context.SendMessageAsync(response.Text, cancellationToken: cancellationToken);
                return response.Text;
            }
        }
    }
}
