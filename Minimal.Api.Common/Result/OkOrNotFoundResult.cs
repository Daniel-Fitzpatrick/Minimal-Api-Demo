using Microsoft.AspNetCore.Http;

namespace Minimal.Api.Common.Result;

public class OkOrNotFoundResult : IResult
{
    private readonly object? _value;

    public OkOrNotFoundResult(object? value)
    {
        _value = value;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        if (_value is null)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            return httpContext.Response.CompleteAsync();
        }

        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        return httpContext.Response.WriteAsJsonAsync(_value);
    }
}