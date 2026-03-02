using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Bacera.Gateway.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private static readonly ActivitySource ActivitySource = new("Bacera.Gateway.Web");

        [HttpGet("trace")]
        public async Task<IActionResult> TestTracing()
        {
            using var activity = ActivitySource.StartActivity("TestTracing");
            activity?.SetTag("test.parameter", "example-value");
            
            // Simulate some work
            await Task.Delay(100);
            
            // Create a child activity
            await DoSomeWork();
            
            activity?.SetTag("test.result", "success");
            
            return Ok(new { message = "Tracing test completed", traceId = activity?.TraceId });
        }

        private async Task DoSomeWork()
        {
            using var activity = ActivitySource.StartActivity("DoSomeWork");
            activity?.SetTag("work.type", "simulation");
            
            await Task.Delay(50);
            
            activity?.SetTag("work.completed", true);
        }
    }
} 