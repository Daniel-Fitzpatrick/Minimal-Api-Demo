using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Minimal.Api.Common.Validation;

namespace Minimal.Api.Common.Result;

public static class ResultExtensions
{
    public static IResult OkayOrNotFound(this IResultExtensions extensions, object? value)
    {
        return new OkOrNotFoundResult(value);
    }

    public static IResult ValidationBadRequest(this IResultExtensions extensions, ValidationResult result)
    {
        return Results.ValidationProblem(result.ToValidationDictionary());
    }

    public static IResult ValidationBadRequest(this IResultExtensions extensions, string propertyName, string validationMessage)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { propertyName, new [] { validationMessage }}
        });
    }
}