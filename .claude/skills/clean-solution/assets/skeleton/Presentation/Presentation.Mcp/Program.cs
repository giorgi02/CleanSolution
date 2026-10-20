using Core.Application;
using Core.Application.Commons;
using Core.Application.Interfaces.Services;
// @if documents
using Infrastructure.Documents;
// @endif
// @if messaging
using Infrastructure.Messaging;
// @endif
using Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// MCP clients are not end users: audit columns are written as "system"
builder.Services.AddScoped<IActiveUserService, SystemActiveUserService>();

// a complete composition root: every layer whose ports the use cases need
// (in Development the container validates every registered handler at startup)
builder.Services.AddApplicationLayer(builder.Configuration);
// @if documents
builder.Services.AddDocumentsLayer(builder.Configuration);
// @endif
// @if messaging
builder.Services.AddMessagingLayer(builder.Configuration);
// @endif
builder.Services.AddPersistenceLayer(builder.Configuration);

builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly(); // every [McpServerToolType] class in this project

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

app.MapMcp("/mcp");

await app.RunAsync();
