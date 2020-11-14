using System;
using System.Threading.Tasks;
using BuildDefinitionProvider.WebApi.DTO.Builds;
using BuildDefinitionProvider.WebApi.Exceptions;
using BuildDefinitionProvider.WebApi.Services.Azure.Builds;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildDefinitionProvider.WebApi.Controllers
{
    [ApiController]
    [Route("api/builds")]
    [Produces("application/json")]
    public class BuildController : ControllerBase
    {
        private readonly IBuildDefinitionService _buildDefinitionService;

        public BuildController(IBuildDefinitionService buildDefinitionService)
        {
            _buildDefinitionService = buildDefinitionService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomBuildDefinitionPayload>> Get(
            string id)
        {
            try
            {
                var definition = await _buildDefinitionService.GetAsync(
                    id);

                if (definition != null)
                {
                    return definition;
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Something went wrong");
            }
            catch (NotFoundException e)
            {
                return NotFound(e);
            }
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    e);
            }
        }

        [HttpPost]
        public async Task<ActionResult<CustomBuildDefinitionId>> Post(
            string project,
            [FromBody] CustomBuildDefinitionPayload payload)
        {
            try
            {
                var id = await _buildDefinitionService.
                    PostAsync(payload);

                return new CustomBuildDefinitionId
                {
                    Id = id
                };
            }
            catch (Exception e)
            {
                return StatusCode(
                     StatusCodes.Status500InternalServerError,
                     e);
            }

        }


        [HttpPut("{id}")]
        public async  Task<ActionResult> Put(
            string id,
            [FromBody] CustomBuildDefinitionPayload payload)
        {
            try
            {
                await _buildDefinitionService.
                    PutAsync(id, payload);

                return NoContent();
            }
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError, 
                    e);
            }
           
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(
            string id)
        {
            try
            {
                await _buildDefinitionService.DeleteAsync(
                    id);

                return NoContent();
            }
            catch (Exception e)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    e);
            }
           
        }
    }
}
