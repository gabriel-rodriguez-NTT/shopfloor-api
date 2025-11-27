using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ShopfloorAssistant.Core.Repository;
using System.Runtime.CompilerServices;

namespace ShopfloorAssistant.Core.AgentsConfig
{
    public class MyAgent : DelegatingAIAgent
    {
        private readonly IThreadRepository threadRepository;
        public MyAgent(AIAgent innerAgent, IThreadRepository threadRepository) : base(innerAgent)
        {
            this.threadRepository = threadRepository;
        }

        public override Task<AgentRunResponse> RunAsync(IEnumerable<ChatMessage> messages, AgentThread? thread = null, AgentRunOptions? options = null, CancellationToken cancellationToken = default)
        {
            return this.RunStreamingAsync(messages, thread, options, cancellationToken).ToAgentRunResponseAsync(cancellationToken);
        }

        public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
            IEnumerable<ChatMessage> messages,
            AgentThread? thread = null,
            AgentRunOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (options is ChatClientAgentRunOptions chatClientAgentRunOptions
                && chatClientAgentRunOptions.ChatOptions?.AdditionalProperties?.TryGetValue<string>("ag_ui_thread_id", out var threadId) == true)
            {
                var threadGuid = Guid.Parse(threadId);
                var newMessages = messages
                .Select(m =>
                {
                    var clone = m.Clone();

                    clone.AdditionalProperties = new AdditionalPropertiesDictionary
                    {
                        ["ag_ui_thread_id"] = threadId
                    };

                    return clone;
                })
                .ToList();

                messages = newMessages;
            }

            return base.RunStreamingAsync(messages, thread, options, cancellationToken);
        }
    }
}
