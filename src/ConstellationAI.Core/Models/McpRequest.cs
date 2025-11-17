namespace ConstellationAI.Core.Models;

/// <summary>
/// 表示一个MCP协议请求
/// </summary>
public class McpRequest
{
    /// <summary>
    /// JSON-RPC版本
    /// </summary>
    public string JsonRpc { get; set; } = "2.0";

    /// <summary>
    /// 请求ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 方法名称
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// 参数
    /// </summary>
    public object? Params { get; set; }
}
