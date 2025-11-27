namespace ShopfloorAssistant.AppService
{
    public interface IThreadAppService
    {
        Task<IEnumerable<ThreadDto>> GetThreadsByUser(string userEmail);
    }
}
