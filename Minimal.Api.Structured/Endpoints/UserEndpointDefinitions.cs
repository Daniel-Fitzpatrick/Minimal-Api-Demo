using FluentValidation;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Result;
using Minimal.Api.Common.Services;

namespace Minimal.Api.Structured.Endpoints
{
    public static class UserEndpointDefinitions
    {
        public static async Task<IResult> CreateUser(User user, IUserService userService, IValidator<User> validator)
        {
            var validationResult = await validator.ValidateAsync(user);
            if (!validationResult.IsValid)
            {
                return Results.Extensions.ValidationBadRequest(validationResult);
            }

            if (await userService.GetByEmailAsync(user.Email) is not null)
            {
                return Results.Extensions.ValidationBadRequest(nameof(User.Email), "User with email address already exists");
            }

            if (await userService.CreateAsync(user))
            {
                return Results.CreatedAtRoute("GetUserById", new { user.Id }, user);
            }

            return Results.Problem("User could not be created");
        }

        public static async Task<IResult> UpdateUser(Guid id, User user, IUserService userService, IValidator<User> validator)
        {
            var validationResult = await validator.ValidateAsync(user);

            if (!validationResult.IsValid)
            {
                return Results.Extensions.ValidationBadRequest(validationResult);
            }

            if (await userService.GetByIdAsync(id) is null)
            {
                return Results.NotFound();
            }

            if ((await userService.GetByEmailAsync(user.Email))?.Id != id.ToString())
            {
                return Results.Extensions.ValidationBadRequest(nameof(User.Email), "User with email address already exists");
            }

            if (await userService.UpdateAsync(id, user))
            {
                return Results.Ok();
            }

            return Results.Problem("User could not be updated");
        }
    }
}
