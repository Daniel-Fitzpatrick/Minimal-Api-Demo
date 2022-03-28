using FluentValidation;
using Minimal.Api.Common;
using Minimal.Api.Common.Data;
using Minimal.Api.Structured.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(
        builder.Configuration.GetValue<string>("Database:ConnectionString")));

builder.Services.AddValidatorsFromAssemblyContaining<ICommonMarker>();

builder.Services.AddUserEndpoints();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.UseUserEndpoints();

app.Run();
