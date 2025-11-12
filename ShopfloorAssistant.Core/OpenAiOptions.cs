namespace ShopfloorAssistant.Core
{
    public class OpenAiOptions
    {
        public const string OpenAI = "OpenAI";
        public string Endpoint { get; set; }
        public string AgentsModel { get; set; }
        public string AgentModelApiKey { get; set; }
        public string TextEmbeddingModel { get; set; }
        public string TextEmbeddingApiKey { get; set; }
    }
}
