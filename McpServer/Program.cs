using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using McpServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

builder.Services.AddHttpClient<ApiService>();


// Ensure that both the Server and the McpServer use the same JSON serialization options to avoid any issues.
builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter() },
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
});

builder.Services.AddSingleton<IValidator<TriviaItem<QuestionDto>>, TriviaQuestionValidator>();

await builder.Build().RunAsync();

