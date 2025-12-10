using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace SampleStorefront.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;
    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("/error")]
    public IActionResult HandleError()
    {
        var exception = HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

        if (exception != null)
        {
            _logger.LogError(exception, "Unhandled exception");
        }

        var statusCode = exception switch
        {
            UnauthorizedAccessException => 401,
            ArgumentException => 400,
            KeyNotFoundException => 404,
            InvalidOperationException => 400,
            _ => 500
        };

        return Problem(
            detail: exception?.Message,
            statusCode: statusCode
        );
    }
}