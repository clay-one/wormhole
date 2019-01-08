using hydrogen.General.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Wormhole.Api.Controllers
{
    [Route("test")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public TestController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("incoming")]
        public IActionResult InsertIncomingMessages([FromBody] object input)
        {
            if (!_hostingEnvironment.IsDevelopment())
            {
                return NotFound();
            }

            return Ok(ApiValidationResult.Ok());
        }
    }
}