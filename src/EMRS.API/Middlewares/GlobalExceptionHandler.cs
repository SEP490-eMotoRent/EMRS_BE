


using System.Net;
using EMRS.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;



namespace API.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        
        httpContext.Response.ContentType = "application/json";
        _logger.LogError(
            exception, "Exception occurred: {Message}", exception.Message);

        var result = ResultResponse<object>.ServerError("An unexpected error occurred");
        httpContext.Response.StatusCode = result.Code switch
        {
            ResultCodes.InternalServerError => StatusCodes.Status500InternalServerError,
            ResultCodes.ServiceUnavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };


        await httpContext.Response
            .WriteAsJsonAsync(result, cancellationToken);

        return true;
    }
    
}