using Minimal.Api.Common.Services;

namespace Minimal.Api.Structured.Endpoints
{
    public static class DeleteUserEndpointDefinition
    {
        public static async Task<IResult> DeleteUser(Guid id, IUserService userService)
        {
            if (await userService.GetByIdAsync(id) is null)
            {
                return Results.NotFound();
            }

            if (await userService.DeleteAsync(id))
            {
                return Results.Ok();
            }

            return Results.Problem("User could not be deleted");
        }
    }
}
