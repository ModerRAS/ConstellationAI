using ConstellationAI.Core.Agents;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace ConstellationAI.Core.Services;

/// <summary>
/// Agent注册表实现
/// </summary>
public class AgentRegistry : IAgentRegistry
{
    private readonly ConcurrentDictionary<string, IAgent> _agentsByPath = new();
    private readonly ConcurrentDictionary<string, IAgent> _agentsById = new();
    private readonly ILogger<AgentRegistry> _logger;

    public AgentRegistry(ILogger<AgentRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterAgent(IAgent agent)
    {
        if (string.IsNullOrWhiteSpace(agent.Id))
        {
            throw new ArgumentException("Agent ID不能为空", nameof(agent));
        }

        if (string.IsNullOrWhiteSpace(agent.UrlPath))
        {
            throw new ArgumentException("Agent URL路径不能为空", nameof(agent));
        }

        // 标准化URL路径
        var normalizedPath = NormalizePath(agent.UrlPath);

        if (_agentsByPath.ContainsKey(normalizedPath))
        {
            throw new InvalidOperationException($"URL路径 '{normalizedPath}' 已被注册");
        }

        if (_agentsById.ContainsKey(agent.Id))
        {
            throw new InvalidOperationException($"Agent ID '{agent.Id}' 已被注册");
        }

        _agentsByPath[normalizedPath] = agent;
        _agentsById[agent.Id] = agent;

        _logger.LogInformation("已注册Agent: {Id} ({Name}) 在路径 {Path}", 
            agent.Id, agent.Name, normalizedPath);
    }

    public IAgent? GetAgentByPath(string urlPath)
    {
        var normalizedPath = NormalizePath(urlPath);
        _agentsByPath.TryGetValue(normalizedPath, out var agent);
        return agent;
    }

    public IAgent? GetAgentById(string id)
    {
        _agentsById.TryGetValue(id, out var agent);
        return agent;
    }

    public IEnumerable<IAgent> GetAllAgents()
    {
        return _agentsById.Values;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        path = path.Trim();
        
        // 确保以 / 开头
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        // 移除末尾的 /
        if (path.Length > 1 && path.EndsWith('/'))
        {
            path = path.TrimEnd('/');
        }

        return path.ToLowerInvariant();
    }
}
