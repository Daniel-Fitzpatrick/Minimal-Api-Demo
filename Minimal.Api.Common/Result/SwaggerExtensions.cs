using Microsoft.AspNetCore.Builder;
using Swashbuckle.AspNetCore.Annotations;

namespace Minimal.Api.Common.Result;

public static class SwaggerExtensions
{
    public static RouteHandlerBuilder WithDescription(this RouteHandlerBuilder builder, string description)
    {
        return builder.WithMetadata(new SwaggerOperationAttribute(description));
    }
}