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

        public AiSearchService(IOptions<AiSearchOptions> aiSearchOptions, IOptions<OpenAiOptions> openAiOptions)
        {
            _aiSearchOptions = aiSearchOptions.Value ?? throw new ArgumentNullException(nameof(aiSearchOptions));
            _openAiOptions = openAiOptions.Value ?? throw new ArgumentNullException(nameof(openAiOptions));

            var credential = new AzureKeyCredential(_aiSearchOptions.APIKey);
            _searchIndexClient = new SearchIndexClient(new Uri(_aiSearchOptions.Endpoint), credential);
        }

        [Description("Executes a search query against an Azure AI Search index using semantic and/or keyword search, and returns the matching results.")]
        public string ExecuteQuery(
            [Description("The user's natural language question or search input. This is used as the query text for the search operation.")] string userQuestion,
            [Description("The name of the Azure AI Search index to execute the query against.")] string searchIndex)
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
                var vectorizedResult = GetEmbeddings(userQuestion);
                searchOptions.VectorSearch = new()
                {
                    Queries = { new VectorizedQuery(vectorizedResult) { KNearestNeighborsCount = vectorTake, Fields = { "content_embedding" } } }
                };

                //Semantic Search (Reranker)
                searchOptions.SemanticSearch = new()
                {
                    SemanticConfigurationName = semanticConfigurationName
                };

                var _searchClient = _searchIndexClient.GetSearchClient(searchIndex);
                SearchResults<AISearchResponse> searchResponse = _searchClient.Search<AISearchResponse>(searchQuery, searchOptions);

                var results = searchResponse.GetResults();

                foreach (SearchResult<AISearchResponse> result in results.Take(take))
                {
                    response.Add(result.Document);
                }

                return JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true // opcional: hace que el JSON se vea legible
                });
            }
            catch (RequestFailedException ex)
            {
                throw;
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

