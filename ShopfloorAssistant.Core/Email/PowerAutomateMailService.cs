using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ShopfloorAssistant.Core.Email
{
    public class PowerAutomateMailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PowerAutomateMailService> _logger;
        private readonly string _flowUrl;

        public PowerAutomateMailService(
            HttpClient httpClient,
            ILogger<PowerAutomateMailService> logger,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _flowUrl = config["PowerAutomate:EndpointWorkflow"]
                ?? throw new InvalidOperationException("Missing PowerAutomate Flow URL in configuration.");
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var payload = new { to, subject, body };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_flowUrl, content);

                _logger.LogInformation("Email request sent to Power Automate: {StatusCode}", response.StatusCode);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email through Power Automate.");
                return false;
            }
        }
    }
}
