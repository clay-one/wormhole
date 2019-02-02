using hydrogen.General.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Wormhole.Api.Controllers
{
    [Route("test-two")]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class TestTwoController : ControllerBase
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public TestTwoController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("incoming")]
        public IActionResult InsertIncomingMessages([FromBody] object input)
        {
            return Ok("Hiiiiiii");
        }
    }
}