using System.Net.Mime;
using HighloadCourse.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace HighloadCourse.ErrorHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var httpResponse = httpContext.Response;

        httpResponse.ContentType = MediaTypeNames.Application.Json;
        httpResponse.StatusCode = StatusCodes.Status500InternalServerError;

        await httpResponse.WriteAsJsonAsync(new ErrorResponse
        {
            Code = StatusCodes.Status500InternalServerError,
            Message = "Internal server error",
            RequestId = httpContext.TraceIdentifier,
        }, cancellationToken);

        return true;
    }
}