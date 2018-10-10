using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;

namespace Wormhole.Api.Controllers
{
    [Route("api/message")]
    [ApiController]
    public class PublishMessageController : ControllerBase
    {
        [HttpPost("publish")]
        public IActionResult Publish([FromBody] PublishInput input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Message))
            {
                return BadRequest(new { message = "parameter is null"});
            }

            var message = input.Message;
            var tenant = input.Tenant;
            var category = input.Category;

            return Ok();
        }
    }
}