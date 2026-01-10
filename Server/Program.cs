using Microsoft.AspNetCore.Http.Json;
using FluentValidation;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using TriviaGame;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>(optional: true);
var connectionString = builder.Configuration.GetConnectionString("Postgres");

// Configure JSON options: camelCase, enums as strings, don't serialize nulls
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});


// Register services
builder.Services
    .AddSignalR()
    // Once again specify JSON options for SignalR since the JsonOptions specified in builder.Services.Configure<JsonOptions>
    // do not apply to SignalR...
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddScoped<IDbConnection>(_ => new Npgsql.NpgsqlConnection(connectionString));
builder.Services.AddSingleton<IValidator<TriviaItem<QuestionDto>>, TriviaQuestionValidator>();
builder.Services.AddSingleton<GameStore>();

var app = builder.Build();

app.UseWebSockets();

// API endpoints
app.MapCrudEndpoints();
app.MapGameEndpoints();
app.MapHub<GameHub>("/gamehub");

// Serve React SPA
app.UseDefaultFiles(); // look for index.html
app.UseStaticFiles();  // serve static files
app.MapFallbackToFile("index.html"); // SPA routing fallback

app.Run();
