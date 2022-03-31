using FluentValidation;
using FluentValidation.AspNetCore;
using Minimal.Api.Common;
using Minimal.Api.Common.Data;
using Minimal.Api.Common.Model;
using Minimal.Api.Common.Result;
using Minimal.Api.Common.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.DisableDataAnnotationsValidation = true;
        fv.RegisterValidatorsFromAssemblyContaining<ICommonMarker>();
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("User1", async (string? skill, IUserService userService) =>
    {
        if (skill is null)
        {
            return Results.Ok(await userService.GetAllAsync());
        }

        return Results.Ok(await userService.SearchBySkill(skill));
    }).WithTags("User")
    .Produces<IEnumerable<User>>()
    .WithDescription("Gets all users or ones matching a skill")
    .ExcludeFromDescription();

app.Run();
