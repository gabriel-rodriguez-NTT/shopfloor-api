using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ShopfloorAssistant.Core.Repository;
using System.Text.Json;

namespace ShopfloorAssistant.Core.ChatStore
{
    public class ShopfloorChatMessageStore : ChatMessageStore
    {
        private readonly IThreadRepository _threadRepository;
        private readonly ShopfloorSession _session;

        public ShopfloorChatMessageStore(IThreadRepository threadRepository, ShopfloorSession session)
        {
            _threadRepository = threadRepository;
            _session = session;
        }

        public override async Task AddMessagesAsync(
            IEnumerable<ChatMessage> messages,
            CancellationToken cancellationToken)
        {
            const string threadIdKey = "ag_ui_thread_id";
            var properties = messages
                .Where(x => x.AdditionalProperties != null && x.AdditionalProperties.ContainsKey(threadIdKey))
                .Select(x => x.AdditionalProperties)
                .FirstOrDefault();
            if (properties?.TryGetValue<string>(threadIdKey, out var threadId) == true && !string.IsNullOrWhiteSpace(_session.UserEmail))
            {
                var threadGuid = Guid.Parse(threadId);
                var threadMessages = messages.Where(m => m.Role != ChatRole.System).Select(m => new Entities.ThreadMessage
                {
                    Id = m.MessageId ?? Guid.NewGuid().ToString(),
                    Role = m.Role.Value,
                    Timestamp = m.CreatedAt ?? DateTimeOffset.UtcNow,
                    Message = m.Text,
                    ThreadId = threadGuid

                });
                await _threadRepository.AddMessagesAsync(threadGuid, _session.UserEmail, threadMessages);
            }
        }

        public override Task<IEnumerable<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null)
        {
            throw new NotImplementedException();
        }
    }

}
