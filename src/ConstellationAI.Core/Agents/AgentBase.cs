using ConstellationAI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace ConstellationAI.Core.Agents;

/// <summary>
/// Agent基类，提供通用的MCP协议处理逻辑
/// </summary>
public abstract class AgentBase : IAgent
{
    protected readonly ILogger Logger;
    protected readonly Kernel? SemanticKernel;

    protected AgentBase(ILogger logger, Kernel? kernel = null)
    {
        Logger = logger;
        SemanticKernel = kernel;
    }

    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string UrlPath { get; }

    public abstract Task<IEnumerable<ToolInfo>> GetToolsAsync();
    public abstract Task<object> ExecuteToolAsync(string toolName, object? parameters);

    public virtual async Task<McpResponse> HandleRequestAsync(McpRequest request)
    {
        try
        {
            Logger.LogInformation("处理MCP请求: {Method}", request.Method);

            object? result = request.Method switch
            {
                "initialize" => await HandleInitializeAsync(request.Params),
                "tools/list" => await HandleToolsListAsync(),
                "tools/call" => await HandleToolsCallAsync(request.Params),
                _ => throw new NotSupportedException($"不支持的方法: {request.Method}")
            };

            return new McpResponse
            {
                Id = request.Id,
                Result = result
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "处理请求时发生错误: {Method}", request.Method);
            return new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32603,
                    Message = ex.Message,
                    Data = ex.StackTrace
                }
            };
        }
    }

    protected virtual Task<object> HandleInitializeAsync(object? parameters)
    {
        return Task.FromResult<object>(new
        {
            protocolVersion = "1.0",
            serverInfo = new
            {
                name = Name,
                version = "1.0.0"
            },
            capabilities = new
            {
                tools = new { }
            }
        });
    }

    protected virtual async Task<object> HandleToolsListAsync()
    {
        var tools = await GetToolsAsync();
        return new { tools };
    }

    protected virtual async Task<object> HandleToolsCallAsync(object? parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters), "工具调用需要参数");
        }

        // 解析参数
        var paramsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
            System.Text.Json.JsonSerializer.Serialize(parameters));

        if (paramsDict == null || !paramsDict.ContainsKey("name"))
        {
            throw new ArgumentException("缺少工具名称参数");
        }

        var toolName = paramsDict["name"].ToString() ?? string.Empty;
        var toolParams = paramsDict.ContainsKey("arguments") ? paramsDict["arguments"] : null;

        var result = await ExecuteToolAsync(toolName, toolParams);

        return new
        {
            content = new[]
            {
                new
                {
                    type = "text",
                    text = System.Text.Json.JsonSerializer.Serialize(result)
                }
            }
        };
    }
}
