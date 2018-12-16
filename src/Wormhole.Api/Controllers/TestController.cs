using System.Threading.Tasks;
using hydrogen.General.Validation;
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
        public async Task<IActionResult> InsertIncomingMessages([FromBody] PublishInput input)
        {
            if (_hostingEnvironment.EnvironmentName != "Development")
            {
                return NotFound();
            }
            return Ok(ApiValidationResult.Ok());
        }
    }
}