using ConstellationAI.Core.Models;

namespace ConstellationAI.Core.Agents;

/// <summary>
/// Agent接口，定义所有Agent必须实现的功能
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Agent的唯一标识符
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Agent的名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Agent的描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Agent的URL路径（例如：/agents/my-agent）
    /// </summary>
    string UrlPath { get; }

    /// <summary>
    /// 获取Agent提供的工具列表
    /// </summary>
    /// <returns>工具信息列表</returns>
    Task<IEnumerable<ToolInfo>> GetToolsAsync();

    /// <summary>
    /// 执行工具调用
    /// </summary>
    /// <param name="toolName">工具名称</param>
    /// <param name="parameters">参数</param>
    /// <returns>执行结果</returns>
    Task<object> ExecuteToolAsync(string toolName, object? parameters);

    /// <summary>
    /// 处理MCP请求
    /// </summary>
    /// <param name="request">MCP请求</param>
    /// <returns>MCP响应</returns>
    Task<McpResponse> HandleRequestAsync(McpRequest request);
}
