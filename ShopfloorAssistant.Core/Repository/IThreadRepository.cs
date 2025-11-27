using ShopfloorAssistant.Core.Entities;

namespace ShopfloorAssistant.Core.Repository
{
    public interface IThreadRepository : IGenericRepository<Entities.Thread>
    {
        Task AddMessagesAsync(Guid threadId, string user, IEnumerable<ThreadMessage> messages);
        Task<IEnumerable<Entities.Thread>> GetUserThreads(string user);
    }
}
