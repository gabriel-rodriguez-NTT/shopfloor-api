using Microsoft.Extensions.AI;

namespace ShopfloorAssistant.Core.Sql
{
    public interface ISqlQueryService
    {
        string ExecuteSqlQuery(string query);
        IEnumerable<AITool> AsAITools();
    }
}
