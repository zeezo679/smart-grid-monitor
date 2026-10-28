using Microsoft.AspNetCore.Mvc;

namespace SmartGrid.Api.Controllers;

/// <summary>
/// Health check endpoint. Indicates the API process is running.
/// Does not verify Snowflake or any external service connectivity.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a simple health status indicating the API is running.
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            Status = "healthy",
            Timestamp = DateTimeOffset.UtcNow,
            Service = "SmartGrid.Api"
        });
    }
}
