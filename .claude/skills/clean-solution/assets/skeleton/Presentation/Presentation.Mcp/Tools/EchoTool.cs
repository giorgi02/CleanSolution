using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Presentation.Mcp.Tools;

/// <summary>
/// Smoke-test tool. Real tools inject use-case handlers through the constructor and call Handle,
/// exactly like controllers do.
/// </summary>
[McpServerToolType]
public sealed class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo([Description("text which should be sent")] string message)
        => $"Hello from C#: {message}";
}
