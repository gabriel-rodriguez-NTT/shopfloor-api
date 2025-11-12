namespace ShopfloorAssistant.Core
{
    public interface IAgentPromptProvider
    {
        Task<string> GetPromptAsync(AgentType agentType, UserRole role);
        Task<string> GetPromptAsync(AgentType agentType);
    }
}
