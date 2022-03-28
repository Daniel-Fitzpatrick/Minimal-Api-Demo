using FluentValidation;
using FluentValidation.AspNetCore;
using Minimal.Api.Common;
using Minimal.Api.Common.Data;
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
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));

builder.Services.AddValidatorsFromAssemblyContaining<ICommonMarker>();

builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
