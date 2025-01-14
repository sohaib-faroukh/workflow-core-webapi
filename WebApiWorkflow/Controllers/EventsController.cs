using Microsoft.AspNetCore.Mvc;
using WorkflowCore.Interface;
using System.Threading.Tasks;

namespace WebApiWorkflow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly IWorkflowController _workflowService;

        public EventsController(IWorkflowController workflowService)
        {
            _workflowService = workflowService;
        }

        [HttpPost("{eventName}/{eventKey}")]
        public async Task<IActionResult> Post(string eventName, string eventKey, [FromBody] object eventData)
        {
            if (eventData == null)
            {
                return BadRequest("Event data is required.");
            }

            await _workflowService.PublishEvent(eventName, eventKey, eventData);
            return Ok();
        }
    }
}
