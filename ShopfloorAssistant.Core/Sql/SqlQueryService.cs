using Microsoft.Data.SqlClient;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Text.Json;

namespace ShopfloorAssistant.Core.Sql
{

    public class SqlQueryService : ISqlQueryService
    {
        private readonly SqlQueryOptions _sqlQueryOptions;
        private readonly ILogger<SqlQueryService> _logger;

        public SqlQueryService(IOptions<SqlQueryOptions> sqlQueryOptions, ILogger<SqlQueryService> logger)
        {
            _sqlQueryOptions = sqlQueryOptions.Value ?? throw new ArgumentNullException(nameof(sqlQueryOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Description("Executes a SQL query using the configured database connection and returns the results as JSON.")]
        public string ExecuteSqlQuery(
            [Description("The SQL query to execute.")] string query
        )
        {
            using (_logger.LogElapsed("[SQL Service]"))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_sqlQueryOptions.ConnectionString))
                        throw new InvalidOperationException("Connection string is not configured.");

                    var results = new List<Dictionary<string, object>>();

                    using (var connection = new SqlConnection(_sqlQueryOptions.ConnectionString))
                    {
                        using (_logger.LogElapsed("-- [SQL Service] Open connection"))
                        {
                            connection.Open();
                        }
                        using (var command = new SqlCommand(query, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            using (_logger.LogElapsed("-- [SQL Service] Reading query results"))
                            {
                                while (reader.Read())
                                {
                                    var row = new Dictionary<string, object>();
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        row[reader.GetName(i)] = reader.GetValue(i);
                                    }
                                    results.Add(row);
                                }
                            }
                        }
                    }
                    using (_logger.LogElapsed("-- [SQL Service] Returning query results"))
                    {
                        return JsonSerializer.Serialize(results, new JsonSerializerOptions
                        {
                            WriteIndented = true // opcional: salida legible
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing SQL query");
                    return JsonSerializer.Serialize(new { error = $"SQL query failed: {ex.Message}" });
                }
            }
        }

        public IEnumerable<AITool> AsAITools()
        {
            yield return AIFunctionFactory.Create(this.ExecuteSqlQuery);
        }
    }
}
