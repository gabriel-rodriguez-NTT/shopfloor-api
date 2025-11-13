using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ShopfloorAssistant.Core
{
    public class FileAgentPromptProvider : IAgentPromptProvider
    {
        private readonly Dictionary<AgentType, string> _agentPromptFiles;
        private readonly Dictionary<UserRole, string> _rolePromptFiles;
        private readonly IUserRoleService _userRoleService;

        public FileAgentPromptProvider(string baseDirectory, IUserRoleService userRoleService)
        {
            _userRoleService = userRoleService;

            _agentPromptFiles = new Dictionary<AgentType, string>
            {
                { AgentType.AiSearchQueryBuilder, Path.Combine(baseDirectory, "ai_search_prompt_builder.txt") },
                { AgentType.AiSearchQueryExecutor, Path.Combine(baseDirectory, "ai_search_prompt_executor.txt") },
                { AgentType.AiSearchQueryAnalyzer, Path.Combine(baseDirectory, "ai_search_prompt_analyzer.txt") },
                { AgentType.SqlBuilder, Path.Combine(baseDirectory, "sql_builder_prompt.txt") },
                { AgentType.SqlExecuter, Path.Combine(baseDirectory, "sql_executer_prompt.txt") },
                { AgentType.SqlAnylizer, Path.Combine(baseDirectory, "sql_anylizer_prompt.txt") },
                { AgentType.Anylizer, Path.Combine(baseDirectory, "analyzer_prompt.txt") }
            };

            _rolePromptFiles = new Dictionary<UserRole, string>
            {
                { UserRole.Operator, Path.Combine(baseDirectory, "role_operator.txt") },
                { UserRole.Supervisor, Path.Combine(baseDirectory, "role_supervisor.txt") },
                { UserRole.Director, Path.Combine(baseDirectory, "role_director.txt") }
            };
        }

        public async Task<string> GetPromptAsync(AgentType agentType)
        {
            return await GetPromptAsync(agentType, _userRoleService.GetCurrentUserRole());
            //return await GetPromptAsync(agentType, UserRole.Operator);
        }

        public async Task<string> GetPromptAsync(AgentType agentType, UserRole role = UserRole.Generic)
        {
            if (!_agentPromptFiles.TryGetValue(agentType, out var agentPromptPath))
                throw new ArgumentException($"No prompt file configured for agent type {agentType}");

            if (!File.Exists(agentPromptPath))
                throw new FileNotFoundException($"Agent prompt file not found: {agentPromptPath}");

            var agentPrompt = await File.ReadAllTextAsync(agentPromptPath);
            string? rolePrompt = null;

            // Solo añadimos prompt de rol si no es "Generic" y existe el archivo
            if (role != UserRole.Generic &&
                _rolePromptFiles.TryGetValue(role, out var rolePromptPath) &&
                File.Exists(rolePromptPath))
            {
                rolePrompt = await File.ReadAllTextAsync(rolePromptPath);
            }

            // Si hay prompt de rol, se concatena antes del prompt del agente
            return rolePrompt is not null
                ? $"{rolePrompt}\n\n{agentPrompt}"
                : agentPrompt;
        }
    }

    public interface IUserRoleService
    {
        UserRole GetCurrentUserRole();
    }

    public class UserRoleService : IUserRoleService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRoleService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserRole GetCurrentUserRole()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return UserRole.Generic;

            // Obtener todos los posibles claims donde puede venir el rol
            var roleClaims = user.FindAll("roles")
                .Concat(user.FindAll("jobTitle"))
                .Concat(user.FindAll("department"))
                .Concat(user.FindAll("extension_role"))
                .Concat(user.FindAll(ClaimTypes.Role))
                .Select(c => c.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();

            if (!roleClaims.Any())
                return UserRole.Generic;

            // Normalizar: separar si un claim tiene varios roles (por coma o espacio)
            var allRoles = roleClaims
                .SelectMany(v => v.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(v => v.Trim().ToLowerInvariant())
                .ToList();

            // Buscar el primero que encaje con tus roles conocidos
            if (allRoles.Contains("production"))
                return UserRole.Operator;

            if (allRoles.Contains("management"))
                return UserRole.Supervisor;

            if (allRoles.Contains("shopfloor director"))
                return UserRole.Director;

            return UserRole.Generic;
        }

    }
}
