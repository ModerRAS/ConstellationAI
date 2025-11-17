namespace ConstellationAI.Core.Models;

/// <summary>
/// 表示一个MCP协议响应
/// </summary>
public class McpResponse
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
    /// 结果数据
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public McpError? Error { get; set; }
}

/// <summary>
/// MCP错误信息
/// </summary>
public class McpError
{
    /// <summary>
    /// 错误代码
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误详情
    /// </summary>
    public object? Data { get; set; }
}
