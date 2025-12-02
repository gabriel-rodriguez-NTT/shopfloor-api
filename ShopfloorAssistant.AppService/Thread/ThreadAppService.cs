using AutoMapper;
using ShopfloorAssistant.AppService;
using ShopfloorAssistant.Core.ChatStore;
using ShopfloorAssistant.Core.Repository;

public class ThreadAppService : IThreadAppService
{
    private readonly IThreadRepository _threadRepository;
    private readonly ShopfloorSession _session;
    private readonly IMapper _mapper;

    public ThreadAppService(IThreadRepository repository, ShopfloorSession session, IMapper mapper)
    {
        _threadRepository = repository;
        _session = session;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ThreadDto>> GetThreadsByUser(string userEmail)
    {
        var threads = await _threadRepository.GetUserThreads(userEmail);

        return _mapper.Map<IEnumerable<ThreadDto>>(threads);
    }

    public async Task<IEnumerable<ThreadMessageDto>> GetThreadsMessages(Guid threadId)
    {
        var thread = await _threadRepository.GetByIdAsync(threadId);
        var messages = thread.Messages.OrderBy(m => m.Timestamp).ToList();

        var result = new List<ThreadMessageDto>();

        foreach (var msg in messages)
        {
            if (msg.Role == "tool" && msg.ToolCalls != null && msg.ToolCalls.Any())
            {
                foreach (var call in msg.ToolCalls)
                {
                    result.Add(new ThreadMessageDto
                    {
                        Id = call.Id.ToString(),
                        ThreadId = msg.ThreadId,
                        Content = msg.Message, // puedes decidir si usar el mensaje original o vacío
                        CreateAt = msg.Timestamp,
                        Role = "tool", // opcional, puedes cambiarlo si quieres
                        ToolCallId = call.CallId,
                        ToolCalls = null // no incluimos ToolCalls aquí
                    });
                }
            }
            else
            {
                // Mensajes normales se envían tal cual
                result.Add(_mapper.Map<ThreadMessageDto>(msg));
            }
        }

        return result;
    }


    public async Task<IEnumerable<ThreadDto>> GetThreadsCurrentUser()
    {
        if (string.IsNullOrWhiteSpace(_session.UserEmail))
            throw new Exception("User email is not set in the session.");

        return await GetThreadsByUser(_session.UserEmail);
    }
}
