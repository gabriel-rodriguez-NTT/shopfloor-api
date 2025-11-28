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

        /// <summary>
        /// Retrieves all threads associated with the specified user.
        /// </summary>
        /// <param name="userEmail">Email of the user.</param>
        /// <returns>List of threads belonging to the user.</returns>
        [HttpGet]
        public async Task<IActionResult> GetThreadsByUser([FromQuery] string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                return BadRequest("Parameter 'userEmail' is required.");

            var threads = await _threadAppService.GetThreadsByUser(userEmail);
            return Ok(threads);
        }

        /// <summary>
        /// Retrieves the threads belonging to the currently authenticated user.
        /// </summary>
        /// <returns>List of threads for the current user.</returns>
        [HttpGet("current-user")]
        public async Task<IActionResult> GetThreadsCurrentUser()
        {
            var threads = await _threadAppService.GetThreadsCurrentUser();
            return Ok(threads);
        }

        /// <summary>
        /// Retrieves all messages associated with a specific thread.
        /// </summary>
        /// <param name="threadId">The ID of the thread.</param>
        /// <returns>List of messages within the specified thread.</returns>
        /// <response code="200">Messages retrieved successfully.</response>
        /// <response code="400">Invalid threadId parameter.</response>
        /// <response code="404">No messages were found for the specified thread.</response>
        [HttpGet("{threadId:guid}/messages")]
        public async Task<IActionResult> GetThreadMessages(Guid threadId)
        {
            if (threadId == Guid.Empty)
                return BadRequest("Parameter 'threadId' is not valid.");

            var messages = await _threadAppService.GetThreadsMessages(threadId);

            if (messages == null || !messages.Any())
                return NotFound($"No messages found for thread {threadId}.");

            return Ok(messages);
        }
    }
}
