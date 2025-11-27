using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ShopfloorAssistant.Core.ChatStore
{
    public class ShopfloorSession
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ShopfloorSession(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// Email del usuario autenticado, si existe.
        /// </summary>
        public string? UserEmail
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return null;

                return user.FindAll(ClaimTypes.Email)
                           .Concat(user.FindAll(ClaimTypes.Upn))
                           .Concat(user.FindAll("preferred_username"))
                           .Select(c => c.Value)
                           .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
            }
        }
    }

}
