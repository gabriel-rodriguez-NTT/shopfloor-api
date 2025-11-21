using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

namespace ShopfloorAssistant.Core.AiSearch
{
    public class AiSearchService : IAiSearchService
    {
        private readonly SearchIndexClient _searchIndexClient;
        private readonly AiSearchOptions _aiSearchOptions;
        private readonly OpenAiOptions _openAiOptions;
        private readonly ILogger<AiSearchService> _logger;

        public AiSearchService(IOptions<AiSearchOptions> aiSearchOptions, IOptions<OpenAiOptions> openAiOptions, ILogger<AiSearchService> logger)
        {
            _aiSearchOptions = aiSearchOptions.Value ?? throw new ArgumentNullException(nameof(aiSearchOptions));
            _openAiOptions = openAiOptions.Value ?? throw new ArgumentNullException(nameof(openAiOptions));

            var credential = new AzureKeyCredential(_aiSearchOptions.APIKey);
            _searchIndexClient = new SearchIndexClient(new Uri(_aiSearchOptions.Endpoint), credential);
            _logger = logger;
        }

        [Description("Use this tool whenever the user asks for information, documents, manuals, reports, or industry facts. It queries the internal knowledge base using semantic search.")]
        public string ExecuteQuery(

    [Description("A specific, descriptive, and semantic search query optimized for retrieval. Do not use vague terms like 'this' or 'it'. Instead of 'how do I fix it?', use 'how to fix pressure valve maintenance'.")]
    string userQuestion,

            [Description("The name of the Azure AI Search index to execute the query against.")] string searchIndex)
        {
            using (_logger.LogElapsed("[AI Search Service]"))
            {
                bool isSemanticEnabled = _aiSearchOptions.SemanticEnabled;
                bool isQuerySearchEnabled = _aiSearchOptions.QuerySearchEnabled;
                string semanticConfigurationName = _aiSearchOptions.SemanticConfigurationName;
                int take = _aiSearchOptions.Take;
                int vectorTake = _aiSearchOptions.VectorTake;

                try
                {
                    var response = new List<AISearchResponse>();

                    SearchQueryType queryType = SearchQueryType.Semantic;

                    SearchOptions searchOptions = GetSearchOptions(queryType);

                    //Query Search
                    string searchQuery = string.Empty;
                    if (isQuerySearchEnabled) searchQuery = userQuestion;

                    //Vector Search
                    using (_logger.LogElapsed("-- [AI Search Service] Vectoring input"))
                    {
                        var vectorizedResult = GetEmbeddings(userQuestion);
                        searchOptions.VectorSearch = new()
                        {
                            Queries = { new VectorizedQuery(vectorizedResult) { KNearestNeighborsCount = vectorTake, Fields = { "content_embedding" } } }
                        };
                    }

                    //Semantic Search (Reranker)
                    searchOptions.SemanticSearch = new()
                    {
                        SemanticConfigurationName = semanticConfigurationName
                    };

                    using (_logger.LogElapsed($"-- [AI Search Service] Executing search in {searchIndex}"))
                    {
                        var _searchClient = _searchIndexClient.GetSearchClient(searchIndex);
                        SearchResults<AISearchResponse> searchResponse = _searchClient.Search<AISearchResponse>(searchQuery, searchOptions);

                        var results = searchResponse.GetResults();

                        foreach (SearchResult<AISearchResponse> result in results.Take(take))
                        {
                            response.Add(result.Document);
                        }
                    }
                    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
                    {
                        WriteIndented = true // opcional: hace que el JSON se vea legible
                    });
                    return json;
                }
                catch (RequestFailedException ex)
                {
                    throw;
                }
            }
        }

        private static SearchOptions GetSearchOptions(SearchQueryType queryType)
        {
            var options = new SearchOptions { QueryType = queryType };
            return options;
        }

        public ReadOnlyMemory<float> GetEmbeddings(string input)
        {
            Uri embeddingEndpoint = new Uri(_openAiOptions.Endpoint);
            string embeddingApiKey = _openAiOptions.TextEmbeddingApiKey;
            string embeddingModel = _openAiOptions.TextEmbeddingModel;

            ApiKeyCredential credentials = new(embeddingApiKey);
            OpenAIClient openAIClient = new(credentials);

            AzureOpenAIClient azureOpenAIClient = new AzureOpenAIClient(embeddingEndpoint, credentials);
            var embeddingClient = azureOpenAIClient.GetEmbeddingClient(embeddingModel);

            var embedding = embeddingClient.GenerateEmbedding(input);

            return embedding.Value.ToFloats();
        }

    }

}

