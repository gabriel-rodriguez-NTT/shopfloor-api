using ShopfloorAssistant.Core.ChatStore;
using ShopfloorAssistant.Core.Repository;
namespace ShopfloorAssistant.AppService
{
    public class ThreadAppService : IThreadAppService
    {
        private readonly IThreadRepository _threadRepository;
        private readonly ShopfloorSession _session;

        public ThreadAppService(IThreadRepository repository, ShopfloorSession session)
        {
            _threadRepository = repository;
            _session = session;
        }

        public async Task<IEnumerable<ThreadDto>> GetThreadsByUser(string userEmail)
        {
            // Trae los threads junto con los mensajes
            var threads = await _threadRepository.GetUserThreads(userEmail);

            // Mapea a DTOs
            var threadDtos = threads.Select(t => new ThreadDto
            {
                Id = t.Id,
                Email = t.User, // usando la propiedad correcta
                Messages = [.. t.Messages.Select(m => new ThreadMessageDto
                {
                    Id = m.Id,
                    ThreadId = m.ThreadId,
                    Content = m.Message,
                    Timestamp = m.Timestamp,
                    Role = m.Role,
                    ToolCallId = m.ToolCallId
                })]
            });

            return [.. threadDtos];
        }

        public async Task<IEnumerable<ThreadDto>> GetThreadsCurrentUser()
        {
            if (string.IsNullOrWhiteSpace(_session.UserEmail))
            {
                throw new Exception("User email is not set in the session.");
            }
            return await this.GetThreadsByUser(_session.UserEmail);
        }
    }
}
