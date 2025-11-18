using ConstellationAI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace ConstellationAI.Core.Agents.Examples;

/// <summary>
/// 示例Agent：回声Agent，用于演示基本功能
/// </summary>
public class EchoAgent : AgentBase
{
    public EchoAgent(ILogger<EchoAgent> logger, Kernel? kernel = null) 
        : base(logger, kernel)
    {
    }

    public override string Id => "echo-agent";
    public override string Name => "Echo Agent";
    public override string Description => "一个简单的回声Agent，用于测试和演示";
    public override string UrlPath => "/agents/echo";

    public override Task<IEnumerable<ToolInfo>> GetToolsAsync()
    {
        var tools = new List<ToolInfo>
        {
            new ToolInfo
            {
                Name = "echo",
                Description = "返回输入的消息",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        message = new
                        {
                            type = "string",
                            description = "要回声的消息"
                        }
                    },
                    required = new[] { "message" }
                }
            },
            new ToolInfo
            {
                Name = "reverse",
                Description = "反转输入的字符串",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        text = new
                        {
                            type = "string",
                            description = "要反转的文本"
                        }
                    },
                    required = new[] { "text" }
                }
            }
        };

        return Task.FromResult<IEnumerable<ToolInfo>>(tools);
    }

    public override Task<object> ExecuteToolAsync(string toolName, object? parameters)
    {
        Logger.LogInformation("执行工具: {ToolName}", toolName);

        return toolName.ToLower() switch
        {
            "echo" => ExecuteEchoAsync(parameters),
            "reverse" => ExecuteReverseAsync(parameters),
            _ => throw new NotSupportedException($"不支持的工具: {toolName}")
        };
    }

    private Task<object> ExecuteEchoAsync(object? parameters)
    {
        if (parameters == null)
        {
            return Task.FromResult<object>(new { message = "" });
        }

        var paramsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
            System.Text.Json.JsonSerializer.Serialize(parameters));

        var message = paramsDict?.ContainsKey("message") == true 
            ? paramsDict["message"].ToString() ?? "" 
            : "";

        Logger.LogInformation("Echo: {Message}", message);

        return Task.FromResult<object>(new { message });
    }

    private Task<object> ExecuteReverseAsync(object? parameters)
    {
        if (parameters == null)
        {
            return Task.FromResult<object>(new { reversed = "" });
        }

        var paramsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
            System.Text.Json.JsonSerializer.Serialize(parameters));

        var text = paramsDict?.ContainsKey("text") == true 
            ? paramsDict["text"].ToString() ?? "" 
            : "";

        var reversed = new string(text.Reverse().ToArray());

        Logger.LogInformation("Reverse: {Text} -> {Reversed}", text, reversed);

        return Task.FromResult<object>(new { reversed });
    }
}
