using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Minimal.Api.Common;
using Minimal.Api.Common.Data;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Result;
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

//extras to show
//named endpoints
//swagger extras
//Result extensions


app.MapGet("User/{id}", async (Guid id, IUserService userService) => Results.Extensions.OkayOrNotFound(await userService.GetByIdAsync(id)))
    .WithName("GetUserById")
    .WithTags("User")
    .Produces<User>()
    .Produces(StatusCodes.Status404NotFound);

app.MapGet("User", async (string? skill, IUserService userService) =>
{
    if (skill is null)
    {
        return Results.Ok(await userService.GetAllAsync());
    }

    return Results.Ok(await userService.SearchBySkill(skill));
}).WithTags("User")
    .Produces<IEnumerable<User>>();

app.MapPost("User", async (User user, IUserService userService, IValidator<User> validator) =>
{
    var validationResult = validator.Validate(user);
    if (validationResult.IsValid)
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
}).WithTags("User")
    .Produces<User>()
    .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapPost("User1", async (User user, IUserService userService, IValidator<User> validator,
    LinkGenerator linkGenerator, HttpContext context) =>
{
    var validationResult = validator.Validate(user);
    if (validationResult.IsValid)
    {
        return Results.Extensions.ValidationBadRequest(validationResult);
    }

    if (await userService.GetByEmailAsync(user.Email) is not null)
    {
        return Results.Extensions.ValidationBadRequest(nameof(User.Email), "User with email address already exists");
    }

    if (await userService.CreateAsync(user))
    {
        var link = linkGenerator.GetUriByName(context, "GetUserById", new { user.Id })!;
        return Results.Created(link, user);
    }

    return Results.Problem("User could not be created");
}).WithTags("User")
    .Produces<User>()
    .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.MapPut("User/{id}", async (Guid id, User user, IUserService userService, IValidator<User> validator) =>
{
    var validationResult = validator.Validate(user);

    if (validationResult.IsValid)
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
}).WithTags("User")
    .Produces<User>()
    .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

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
}).WithTags("User")
    .Produces<User>()
    .Produces(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError);

app.Run();
