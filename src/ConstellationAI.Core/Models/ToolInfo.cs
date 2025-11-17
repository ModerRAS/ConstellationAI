namespace ConstellationAI.Core.Models;

/// <summary>
/// 工具信息，描述Agent能力
/// </summary>
public class ToolInfo
{
    /// <summary>
    /// 工具名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 工具描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 输入模式（JSON Schema）
    /// </summary>
    public object? InputSchema { get; set; }
}
