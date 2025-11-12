namespace ShopfloorAssistant.Core.AiSearch
{
    public class AiSearchOptions
    {
        // La clave de configuración principal (debe coincidir con la sección en appsettings.json)
        public const string AzureAISearch = "AzureAISearch";

        public string Auth { get; set; } = "ApiKey";
        public string Endpoint { get; set; } = string.Empty;
        public string APIKey { get; set; } = "To be resolved";

        // Los tipos booleanos y numéricos se cargan automáticamente
        public bool UseHybridSearch { get; set; } = false;
        public bool UseStickySessions { get; set; } = false;
        public int Take { get; set; } = 10; // Usar int o string, dependiendo de cómo lo uses
        public int VectorTake { get; set; } = 10; // Usar int o string

        public bool DescriptionEnabled { get; set; } = false;
        public bool QuerySearchEnabled { get; set; } = false;
        public bool SemanticEnabled { get; set; } = false;
        public string SemanticConfigurationName { get; set; } = string.Empty;
    }
}
