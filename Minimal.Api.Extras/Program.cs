using FluentValidation;
using Minimal.Api.Common;
using Minimal.Api.Common.Data;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Result;
using Minimal.Api.Common.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

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
//Other http verbs


app.MapGet("User/{id}", async (Guid id, IUserService userService) => Results.Extensions.OkayOrNotFound(await userService.GetByIdAsync(id)))
    .WithName("GetUserById")
    .WithTags("User")
    .Produces<User>()
    .Produces(StatusCodes.Status404NotFound)
    .WithDescription("Get User by Id");

app.MapGet("User", async (string? skill, IUserService userService) =>
{
    if (skill is null)
    {
        return Results.Ok(await userService.GetAllAsync());
    }

    return Results.Ok(await userService.SearchBySkill(skill));
}).WithTags("User")
    .Produces<IEnumerable<User>>()
    .WithDescription("Gets all users or ones matching a skill");

app.MapPost("User", async (User user, IUserService userService, IValidator<User> validator) =>
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
}).WithTags("User")
    .Produces<User>()
    .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .WithDescription("Creates a new User");

app.MapPost("User1", async (User user, IUserService userService, IValidator<User> validator,
    LinkGenerator linkGenerator, HttpContext context) =>
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
        var link = linkGenerator.GetUriByName(context, "GetUserById", new { user.Id })!;
        return Results.Created(link, user);
    }

    return Results.Problem("User could not be created");
}).WithTags("User")
    .Produces<User>()
    .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .WithDescription("Creates a new User");

app.MapPut("User/{id}", async (Guid id, User user, IUserService userService, IValidator<User> validator) =>
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
}).WithTags("User")
    .Produces<User>()
    .Produces<Dictionary<string, string[]>>(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .WithDescription("Updates a User");

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
    .ProducesProblem(StatusCodes.Status500InternalServerError)
    .WithDescription("Deletes a User");

app.MapMethods("User", new[] { "PATCH", "OPTIONS", "HEAD" }, (HttpContext context) => $"this is a {context.Request.Method} request");

app.Run();
