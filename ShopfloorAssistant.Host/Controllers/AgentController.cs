using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using ShopfloorAssistant.AppService;
using Microsoft.AspNetCore.Authorization;

namespace ShopfloorAssistant.Host.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public partial class AgentController : ControllerBase
    {
        private readonly IAgentAppService _agentAppService;

        public AgentController(IAgentAppService agentAppService)
        {
            _agentAppService = agentAppService;
        }

        [HttpPost("mcp")]
        public async Task<ActionResult<AskResponse>> Mcp([FromBody] AskRequest request)
        {
            var answer = await _agentAppService.RunMcpTest(request.Question);

            return Ok(new AskResponse { Answer = answer });
        }

        [HttpGet("ai-search-workflow")]
        public async Task<IActionResult> AiSearchWorkflow([FromQuery] string message)
        {
            var result = await _agentAppService.RunWorkflowAsync("ai-search", message);
            return Ok(result);
        }

        [HttpGet("sql-search-workflow")]
        public async Task<IActionResult> SqlWorkflow([FromQuery] string message)
        {
            var result = await _agentAppService.RunWorkflowAsync("sql-search", message);
            return Ok(result);
        }

        [HttpGet("concurrent-workflow")]
        public async Task<IActionResult> Concurrent([FromQuery] string message)
        {
            var result = await _agentAppService.RunWorkflowAsync("concurrent", message);
            return Ok(result);
        }

        [HttpGet("tool-workflow")]
        public async Task<IActionResult> Tool([FromQuery] string message)
        {
            var result = await _agentAppService.RunWorkflowAsync("tool", message);
            return Ok(result);
        }
    }
}
