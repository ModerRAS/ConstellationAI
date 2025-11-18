using ConstellationAI.Core.Agents;

namespace ConstellationAI.Core.Services;

/// <summary>
/// Agent注册表接口
/// </summary>
public interface IAgentRegistry
{
    /// <summary>
    /// 注册一个Agent
    /// </summary>
    /// <param name="agent">要注册的Agent</param>
    void RegisterAgent(IAgent agent);

    /// <summary>
    /// 根据URL路径获取Agent
    /// </summary>
    /// <param name="urlPath">URL路径</param>
    /// <returns>找到的Agent，如果不存在则返回null</returns>
    IAgent? GetAgentByPath(string urlPath);

    /// <summary>
    /// 根据ID获取Agent
    /// </summary>
    /// <param name="id">Agent ID</param>
    /// <returns>找到的Agent，如果不存在则返回null</returns>
    IAgent? GetAgentById(string id);

    /// <summary>
    /// 获取所有已注册的Agent
    /// </summary>
    /// <returns>所有Agent的集合</returns>
    IEnumerable<IAgent> GetAllAgents();
}
