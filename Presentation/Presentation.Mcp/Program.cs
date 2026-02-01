using Presentation.Mcp.Tools;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5555");

builder.Services.AddMcpServer()
    .WithHttpTransport((options) =>
    {
        options.Stateless = true;
    })
    .WithToolsFromAssembly();
    //.WithTools<EchoTool>();

builder.Logging.AddConsole(options =>
{
    options.LogToStandardErrorThreshold = LogLevel.Trace;
});

var app = builder.Build();

app.MapMcp("/mcp");

await app.RunAsync();