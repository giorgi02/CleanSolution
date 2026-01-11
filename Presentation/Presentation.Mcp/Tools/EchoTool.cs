using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Presentation.Mcp.Tools;

[McpServerToolType]
public class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo([Description("text which should be sent")] string message) => $"Hello from C#: {message}";

    [McpServerTool, Description("Reverses the message.")]
    public static string ReverseEcho([Description("text which should be sent")] string message)
        => new string(message.Reverse().ToArray());
}