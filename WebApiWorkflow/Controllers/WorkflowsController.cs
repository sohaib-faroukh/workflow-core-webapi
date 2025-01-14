using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.Search;

namespace WebApiWorkflow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowController _workflowService;
        private readonly IWorkflowRegistry _registry;
        private readonly IPersistenceProvider _workflowStore;
        private readonly ISearchIndex _searchService;

        public WorkflowsController(
            IWorkflowController workflowService,
            ISearchIndex searchService,
            IWorkflowRegistry registry,
            IPersistenceProvider workflowStore)
        {
            _workflowService = workflowService;
            _workflowStore = workflowStore;
            _registry = registry;
            _searchService = searchService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            string terms,
            WorkflowStatus? status,
            string type,
            DateTime? createdFrom,
            DateTime? createdTo,
            int skip,
            int take = 10)
        {
            var filters = new List<SearchFilter>();

            if (status.HasValue)
                filters.Add(StatusFilter.Equals(status.Value));

            if (createdFrom.HasValue)
                filters.Add(DateRangeFilter.After(x => x.CreateTime, createdFrom.Value));

            if (createdTo.HasValue)
                filters.Add(DateRangeFilter.Before(x => x.CreateTime, createdTo.Value));

            if (!string.IsNullOrEmpty(type))
                filters.Add(ScalarFilter.Equals(x => x.WorkflowDefinitionId, type));

            var result = await _searchService.Search(terms, skip, take, filters.ToArray());

            return Ok(result); // Replaced Json(result) with Ok(result)
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _workflowStore.GetWorkflowInstance(id);
            return Ok(result); // Replaced Json(result) with Ok(result)
        }

        [HttpPost("{id}")]
        [HttpPost("{id}/{version}")]
        public async Task<IActionResult> Post(
            string id,
            int? version,
            [FromBody] JObject data)
        {
            string workflowId = null;
            var def = _registry.GetDefinition(id, version);
            if (def == null)
                return BadRequest($"Workflow definition {id} for version {version} not found");

            if (data != null && def.DataType != null)
            {
                var dataStr = JsonConvert.SerializeObject(data);
                var dataObj = JsonConvert.DeserializeObject(dataStr, def.DataType);
                workflowId = await _workflowService.StartWorkflow(id, version, dataObj);
            }
            else
            {
                workflowId = await _workflowService.StartWorkflow(id, version, null);
            }

            return Ok(workflowId);
        }

        [HttpPut("{id}/suspend")]
        public async Task<IActionResult> Suspend(string id)
        {
            var result = await _workflowService.SuspendWorkflow(id);
            return Ok(result);
        }

        [HttpPut("{id}/resume")]
        public async Task<IActionResult> Resume(string id)
        {
            var result = await _workflowService.ResumeWorkflow(id);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Terminate(string id)
        {
            var result = await _workflowService.TerminateWorkflow(id);
            return Ok(result);
        }
    }
}
