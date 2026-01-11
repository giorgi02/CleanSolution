using Presentation.Mcp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5280");

builder.Services.AddMcpServer()
    .WithHttpTransport((options) =>
    {
        options.Stateless = true;
    })
    .WithTools<EchoTool>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

app.MapMcp();

await app.RunAsync();