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

        return _mapper.Map<IEnumerable<ThreadMessageDto>>(thread.Messages);
    }

    public async Task<IEnumerable<ThreadDto>> GetThreadsCurrentUser()
    {
        if (string.IsNullOrWhiteSpace(_session.UserEmail))
            throw new Exception("User email is not set in the session.");

        return await GetThreadsByUser(_session.UserEmail);
    }
}
