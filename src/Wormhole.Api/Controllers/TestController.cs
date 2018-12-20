using Hydrogen.General.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Wormhole.Api.Model;

namespace Wormhole.Api.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public TestController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("incoming")]
        public IActionResult InsertIncomingMessages([FromBody] PublishInput input)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            return Ok(ApiValidationResult.Ok());
        }
    }
}