using Markdig;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShopfloorAssistant.Core.Email
{
    public class PowerAutomateMailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly string _flowUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PowerAutomateMailService(
            HttpClient httpClient,
            IConfiguration config,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
            _flowUrl = config["PowerAutomate:EndpointWorkflow"]
                ?? throw new InvalidOperationException("Missing PowerAutomate Flow URL in configuration.");
        }

        [Description("Sends an email message to a chosen recipient. This function prepares the message, enriches it with the provided subject and content, and dispatches it through the system’s email delivery mechanism.")]
        public async Task<bool> SendEmailAsync(
        [Description("The email address of the intended recipient. If omitted, a default address may be used.")] string? to,
        [Description("A short line summarizing the purpose or theme of the message.")] string subject,
        [Description("The main content of the email. This may include plain text, structured text, markdown or HTML-formatted content.")] string body)
        {
            try
            {
                if (to == "not@provided.com")
                {
                    to = GetEmailFromToken();
                }
                var payload = new { to, subject, body };
                var bodyHtml = Markdown.ToHtml(body);
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_flowUrl, content);

                Console.WriteLine($"Email request sent to Power Automate: {response.StatusCode}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private string? GetEmailFromToken()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            // Obtener todos los posibles claims donde puede venir el rol
            return user.FindAll(ClaimTypes.Email)
                .Concat(user.FindAll(ClaimTypes.Upn))
                 .Select(c => c.Value)
                 .FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
        }
    }
}
