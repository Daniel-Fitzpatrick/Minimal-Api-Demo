using Minimal.Api.Common.Model;
using Minimal.Api.Common.Result;
using Minimal.Api.Common.Services;

namespace Minimal.Api.Structured.Endpoints
{
    public static class UserEndpoints
    {
        public static void AddUserEndpoints(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
        }

        public static void UseUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("User/{id}", GetUserById)
                .WithName("GetUserById")
                .WithTags("User")
                .Produces<User>()
                .Produces(StatusCodes.Status404NotFound);

            app.MapGet("User", GetUsers).WithTags("User")
                .Produces<IEnumerable<User>>();

            app.MapPost("User", UserEndpointDefinitions.CreateUser).WithTags("User")
                .Produces<User>()
                .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapPut("User/{id}", UserEndpointDefinitions.UpdateUser).WithTags("User")
                .Produces<User>()
                .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

            app.MapDelete("User/{id}", DeleteUserEndpointDefinition.DeleteUser).WithTags("User")
                .Produces<User>()
                .Produces(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }


        private static async Task<IResult> GetUserById(Guid id, IUserService userService) =>
            Results.Extensions.OkayOrNotFound(await userService.GetByIdAsync(id));

        private static async Task<IResult> GetUsers(string? skill, IUserService userService)
        {
            return skill is null ? Results.Ok(await userService.GetAllAsync()) : Results.Ok(await userService.SearchBySkill(skill));
        }
    }
}
