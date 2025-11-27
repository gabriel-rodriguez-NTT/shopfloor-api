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
    public class ThreadsController : ControllerBase
    {
        private readonly IThreadAppService _threadAppService;

        public ThreadsController(IThreadAppService threadAppService)
        {
            _threadAppService = threadAppService;
        }

        [HttpGet]
        public async Task<IActionResult> GetThreadsByUser([FromQuery] string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                return BadRequest("El parámetro 'userEmail' es obligatorio.");

            var threads = await _threadAppService.GetThreadsByUser(userEmail);
            return Ok(threads);
        }

        [HttpGet("current-user")]
        public async Task<IActionResult> GetThreadsCurrentUser()
        {
            var threads = await _threadAppService.GetThreadsCurrentUser();
            return Ok(threads);
        }
    }

}
