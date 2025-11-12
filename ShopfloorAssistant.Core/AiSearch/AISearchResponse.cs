using System.Text.Json.Serialization;

namespace ShopfloorAssistant.Core.AiSearch
{
    public class AISearchResponse
    {
        [JsonPropertyName("@search.score")]
        [JsonPropertyOrder(1)]
        public float SearchScore { get; set; }

        [JsonPropertyName("content_id")]
        [JsonPropertyOrder(2)]
        public string ContentId { get; set; }

        [JsonPropertyName("text_document_id")]
        [JsonPropertyOrder(3)]
        public string TextDocumentId { get; set; }

        [JsonPropertyName("document_title")]
        [JsonPropertyOrder(4)]
        public string DocumentTitle { get; set; }

        [JsonPropertyName("content_text")]
        [JsonPropertyOrder(5)]
        public string ContentText { get; set; }
    }
}
