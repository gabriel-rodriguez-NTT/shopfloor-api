using ShopfloorAssistant.Core.Repository;
namespace ShopfloorAssistant.AppService
{
    public class ThreadAppService : IThreadAppService
    {
        private readonly IThreadRepository _threadRepository;

        public ThreadAppService(IThreadRepository context)
        {
            _threadRepository = context;
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
                Messages = t.Messages.Select(m => new ThreadMessageDto
                {
                    Id = m.Id,
                    ThreadId = m.ThreadId,
                    Content = m.Message,
                    Timestamp = m.Timestamp,
                    Role = m.Role
                }).ToList()
            });

            return threadDtos.ToList();
        }

    }
}
