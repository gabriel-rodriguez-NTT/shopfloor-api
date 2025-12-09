namespace ShopfloorAssistant.Core
{
    public class McpOptions
    {
        public const string Mcp = "Mcp";
        public string Endpoint { get; set; }
        public string Name { get; set; }
        public string Instructions { get; set; }
        public string ModelName { get; set; }
        public string AllowedTools { get; set; }
    }
}
