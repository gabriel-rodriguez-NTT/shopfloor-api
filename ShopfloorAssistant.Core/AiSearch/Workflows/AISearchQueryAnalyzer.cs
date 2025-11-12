using Microsoft.Agents.AI.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace ShopfloorAssistant.Core.Workflows
{
    internal sealed class AISearchQueryAnalyzer : Executor<AiSearchQueryResult>
    {
        private readonly AIAgent _agent;
        private readonly AgentThread _thread;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="instructions"></param>
        /// <param name="id"></param>
        /// <param name="chatClient"></param>
        public AISearchQueryAnalyzer(string instructions, string id, IChatClient chatClient) : base(id)
        {

            ChatClientAgentOptions agentOptions = new(
                instructions: instructions)
            {

            };

            _agent = new ChatClientAgent(chatClient, agentOptions);
            _thread = _agent.GetNewThread();
        }

        public override async ValueTask HandleAsync(AiSearchQueryResult result, IWorkflowContext context, CancellationToken cancellationToken = default)
        {
            await context.YieldOutputAsync($"[AISearch Agent (Analyzer)]: Analyzing AI Search results...", cancellationToken);
            var input = $"""
                User question: {result.UserInput}
                AiSearchResult: {result.AiSearchResult}
                """;

            var response = await _agent.RunAsync(input, _thread, cancellationToken: cancellationToken);

            await context.AddEventAsync(new AiSearchEvent(response.Text), cancellationToken);

            await context.SendMessageAsync(response.Text, cancellationToken: cancellationToken);
        }
    }
}
