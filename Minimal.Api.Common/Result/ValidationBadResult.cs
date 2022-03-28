using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Minimal.Api.Common.Validation;

namespace Minimal.Api.Common.Result;

public class ValidationBadResult : IResult
{
    private readonly ValidationResult? _value;

    public ValidationBadResult(ValidationResult? value)
    {
        _value = value;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return _value is not null ? httpContext.Response.WriteAsJsonAsync(_value.ToValidationDictionary()) : httpContext.Response.CompleteAsync();
    }
}