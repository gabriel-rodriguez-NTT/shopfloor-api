using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using ShopfloorAssistant.Core.Entities;
using ShopfloorAssistant.Core.Repository;
using System.Text.Json;

namespace ShopfloorAssistant.Core.ChatStore
{
    public class ShopfloorChatMessageStore : ChatMessageStore
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ShopfloorSession _session;

        public ShopfloorChatMessageStore(IServiceProvider serviceProvider, ShopfloorSession session)
        {
            _serviceProvider = serviceProvider;
            _session = session;
        }

        public IThreadRepository GetThreadRepository()
        {
            // Resuelve el IThreadRepository cuando lo necesites
            return _serviceProvider.GetRequiredService<IThreadRepository>();
        }

        //public override async Task AddMessagesAsync(
        //    IEnumerable<ChatMessage> messages,
        //    CancellationToken cancellationToken)
        //{
        //    const string threadIdKey = "ag_ui_thread_id";
        //    var properties = messages
        //        .Where(x => x.AdditionalProperties != null && x.AdditionalProperties.ContainsKey(threadIdKey))
        //        .Select(x => x.AdditionalProperties)
        //        .FirstOrDefault();
        //    if (properties?.TryGetValue<string>(threadIdKey, out var threadId) == true && !string.IsNullOrWhiteSpace(_session.UserEmail))
        //    {
        //        var threadGuid = Guid.Parse(threadId);
        //        var threadMessages = messages.Where(m => m.Role != ChatRole.System).Select(m => new Entities.ThreadMessage
        //        {
        //            Id = m.MessageId ?? Guid.NewGuid().ToString(),
        //            Role = m.Role.Value,
        //            Timestamp = m.CreatedAt ?? DateTimeOffset.UtcNow,
        //            Message = m.Text,
        //            //ThreadId = threadGuid,
        //            //ToolCallId = m.Contents.Where(x => x is FunctionResultContent).Select(x => x as FunctionResultContent).FirstOrDefault()?.CallId,
        //            ToolCalls = [.. m.Contents.Where(x => x is FunctionCallContent).Select(x => x as FunctionCallContent).Select(x => new Entities.ThreadToolCall()
        //            {
        //                Id = Guid.NewGuid(),
        //                CallId = x.CallId,
        //                Name = x.Name,
        //                Arguments = x.Arguments
        //            }),
        //            ..m.Contents.Where(x => x is FunctionResultContent).Select(x => x as FunctionResultContent).Select(x => new Entities.ThreadToolCall()
        //            {
        //                Id = Guid.NewGuid(),
        //                CallId = x.CallId,
        //                Name = messages.SelectMany(y => y.Contents).Where(y => y is FunctionCallContent).Select(y => y as FunctionCallContent).FirstOrDefault(y => y.CallId == x.CallId)?.Name 
        //                ?? nameof(FunctionResultContent),
        //                Result = x.Result?.ToString()
        //            })]
        //        }).ToList();
        //        foreach (var msg in threadMessages)
        //        {
        //            foreach (var call in msg.ToolCalls)
        //            {
        //                call.ThreadMessage = null;
        //                call.ThreadMessageId = msg.Id;
        //            }
        //        }
        //        await _threadRepository.AddMessagesAsync(threadGuid, _session.UserEmail, threadMessages);
        //    }
        //}

        public override async Task AddMessagesAsync(
    IEnumerable<ChatMessage> messages,
    CancellationToken cancellationToken)
        {
            const string threadIdKey = "ag_ui_thread_id";
            var repository = GetThreadRepository();
            // Obtener ThreadId de los mensajes
            var properties = messages
                .Where(x => x.AdditionalProperties != null && x.AdditionalProperties.ContainsKey(threadIdKey))
                .Select(x => x.AdditionalProperties)
                .FirstOrDefault();

            if (properties?.TryGetValue<string>(threadIdKey, out var threadId) == true
                && !string.IsNullOrWhiteSpace(_session.UserEmail))
            {
                var threadGuid = Guid.Parse(threadId);

                // Convertir ChatMessages a ThreadMessages
                var threadMessages = messages
                    .Where(m => m.Role != ChatRole.System)
                    .Select(m =>
                    {
                        var tm = new ThreadMessage
                        {
                            Id = m.MessageId ?? Guid.NewGuid().ToString(),
                            Role = m.Role.Value,
                            Timestamp = m.CreatedAt ?? DateTimeOffset.UtcNow,
                            Message = m.Text,
                            ToolCalls = new List<ThreadToolCall>()
                        };

                        // Reconstruir ToolCalls
                        foreach (var content in m.Contents)
                        {
                            if (content is FunctionCallContent call)
                            {
                                tm.ToolCalls.Add(new ThreadToolCall
                                {
                                    //Id = Guid.NewGuid(),
                                    CallId = call.CallId,
                                    Name = call.Name,
                                    Arguments = call.Arguments,
                                    ThreadMessageId = tm.Id,
                                    ThreadMessage = null
                                });
                            }
                            else if (content is FunctionResultContent result)
                            {
                                tm.ToolCalls.Add(new ThreadToolCall
                                {
                                    //Id = Guid.NewGuid(),
                                    CallId = result.CallId,
                                    Name = messages.SelectMany(y => y.Contents).Where(y => y is FunctionCallContent).Select(y => y as FunctionCallContent).FirstOrDefault(y => y.CallId == result.CallId)?.Name ?? nameof(FunctionResultContent),
                                    Result = result.Result?.ToString(),
                                    ThreadMessageId = tm.Id,
                                    ThreadMessage = null
                                });
                            }
                        }

                        return tm;
                    })
                    .ToList();

                // Llamar al repositorio con ThreadMessages ya listos
                await repository.AddMessagesAsync(threadGuid, _session.UserEmail, threadMessages);
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
