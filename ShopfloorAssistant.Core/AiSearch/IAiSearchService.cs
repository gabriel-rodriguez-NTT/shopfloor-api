namespace ShopfloorAssistant.Core.AiSearch
{
    public interface IAiSearchService
    {
        string ExecuteQuery(string userQuestion, string searchIndex);
    }

}

