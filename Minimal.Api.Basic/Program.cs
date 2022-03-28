using FluentValidation;
using Minimal.Api.Common;
using Minimal.Api.Common.Data;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Services;
using Minimal.Api.Common.Validation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));

builder.Services.AddValidatorsFromAssemblyContaining<ICommonMarker>();

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.MapGet("User/{id}", async (Guid id, IUserService userService) =>
{
    var user = await userService.GetByIdAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

app.MapGet("User", async (string? skill, IUserService userService) =>
{
    if (skill is null)
    {
        return Results.Ok(await userService.GetAllAsync());
    }

    return Results.Ok(await userService.SearchBySkill(skill));
});

app.MapPost("User", async (User user, IUserService userService, IValidator<User> validator) =>
{
    var validationResult = await validator.ValidateAsync(user);
    if (validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.ToValidationDictionary());
    }

    if (await userService.GetByEmailAsync(user.Email) is not null)
    {
        return Results.BadRequest(new Dictionary<string, string[]> { { nameof(User.Email), new[] { "User with email address already exists" } } });
    }

    if (await userService.CreateAsync(user))
    {
        return Results.Created($"User/{user.Id}", user);
    }

    return Results.Problem("User could not be created");
});

app.MapPut("User/{id}", async (Guid id, User user, IUserService userService, IValidator<User> validator) =>
{
    var validationResult = await validator.ValidateAsync(user);

    if (validationResult.IsValid)
    {
        return Results.BadRequest(validationResult.ToValidationDictionary());
    }

    if (await userService.GetByIdAsync(id) is null)
    {
        return Results.NotFound();
    }

    if ((await userService.GetByEmailAsync(user.Email))?.Id != id.ToString())
    {
        return Results.BadRequest(new Dictionary<string, string[]> { { nameof(User.Email), new[] { "User with email address already exists" } } });
    }

    if (await userService.UpdateAsync(id, user))
    {
        return Results.Ok();
    }

    return Results.Problem("User could not be updated");
});

app.MapDelete("User/{id}", async (Guid id, IUserService userService) =>
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
});

app.Run();
