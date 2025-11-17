# 架构设计文档

## 概述

ConstellationAI是一个基于C#和Semantic Kernel的MCP（Model Context Protocol）服务器框架，允许创建和部署多个智能体（Agent），每个Agent都可以独立响应MCP协议请求。

## 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      LLM / MCP Client                       │
└───────────────────────────┬─────────────────────────────────┘
                            │ MCP Protocol (JSON-RPC 2.0)
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              ConstellationAI MCP Server                     │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           ASP.NET Core Web Server                   │   │
│  │  ┌──────────────────────────────────────────────┐   │   │
│  │  │         HTTP Endpoints                        │   │   │
│  │  │  • GET  /              (服务器信息)            │   │   │
│  │  │  • POST /{path}        (MCP请求路由)          │   │   │
│  │  └──────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                            │                                │
│                            ▼                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │           Agent Registry (注册表)                   │   │
│  │  • 管理所有已注册的Agent                             │   │
│  │  • 按URL路径或ID查找Agent                           │   │
│  │  • 防止重复注册                                      │   │
│  └─────────────────────────────────────────────────────┘   │
│                            │                                │
│         ┌──────────────────┼──────────────────┐            │
│         ▼                  ▼                  ▼            │
│  ┌───────────┐      ┌───────────┐      ┌───────────┐      │
│  │  Agent 1  │      │  Agent 2  │      │  Agent N  │      │
│  │  /path1   │      │  /path2   │      │  /pathN   │      │
│  │           │      │           │      │           │      │
│  │ Tools:    │      │ Tools:    │      │ Tools:    │      │
│  │ • tool1   │      │ • toolA   │      │ • toolX   │      │
│  │ • tool2   │      │ • toolB   │      │ • toolY   │      │
│  └─────┬─────┘      └─────┬─────┘      └─────┬─────┘      │
│        │                  │                  │            │
│        └──────────────────┼──────────────────┘            │
│                           ▼                                │
│  ┌─────────────────────────────────────────────────────┐   │
│  │         Semantic Kernel (可选)                      │   │
│  │  • AI模型集成 (OpenAI, Azure OpenAI, etc.)         │   │
│  │  • Prompt管理                                       │   │
│  │  • 插件系统                                         │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## 核心组件

### 1. MCP Server (ASP.NET Core)

**职责:**
- 处理HTTP请求
- 路由MCP请求到相应的Agent
- 提供服务器信息端点

**关键端点:**
- `GET /` - 返回服务器信息和已注册Agent列表
- `POST /{*path}` - 处理所有MCP协议请求

### 2. Agent Registry

**职责:**
- 注册和管理所有Agent实例
- 提供Agent查找功能（按路径或ID）
- 确保URL路径和ID的唯一性

**接口:**
```csharp
public interface IAgentRegistry
{
    void RegisterAgent(IAgent agent);
    IAgent? GetAgentByPath(string urlPath);
    IAgent? GetAgentById(string id);
    IEnumerable<IAgent> GetAllAgents();
}
```

### 3. Agent

**职责:**
- 实现MCP协议处理
- 提供工具（Tools）定义
- 执行工具调用
- 可选地集成Semantic Kernel

**接口:**
```csharp
public interface IAgent
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string UrlPath { get; }
    
    Task<IEnumerable<ToolInfo>> GetToolsAsync();
    Task<object> ExecuteToolAsync(string toolName, object? parameters);
    Task<McpResponse> HandleRequestAsync(McpRequest request);
}
```

**Agent生命周期:**
```
1. 创建 → 2. 注册到Registry → 3. 接收请求 → 4. 处理请求 → 5. 返回响应
```

### 4. MCP Protocol Models

**McpRequest:**
```csharp
{
    "jsonrpc": "2.0",
    "id": "request-id",
    "method": "method-name",
    "params": { }
}
```

**McpResponse:**
```csharp
{
    "jsonrpc": "2.0",
    "id": "request-id",
    "result": { },
    "error": { }
}
```

### 5. Semantic Kernel集成

Semantic Kernel是可选的，为Agent提供AI能力：
- 与LLM交互
- Prompt工程和模板
- 插件系统
- 内存管理

## 请求处理流程

```
1. 客户端发送HTTP POST请求到 /agents/echo
   ↓
2. ASP.NET Core接收请求并解析JSON-RPC消息
   ↓
3. 从URL路径提取Agent路径 (/agents/echo)
   ↓
4. 通过AgentRegistry查找对应的Agent
   ↓
5. Agent.HandleRequestAsync() 处理请求
   ↓
6. 根据method分发到相应的处理器:
   • initialize → HandleInitializeAsync()
   • tools/list → HandleToolsListAsync()
   • tools/call → HandleToolsCallAsync()
   ↓
7. 如果是tools/call，调用 ExecuteToolAsync()
   ↓
8. 返回McpResponse
   ↓
9. 序列化为JSON并返回给客户端
```

## MCP协议支持

### 支持的方法

1. **initialize**
   - 初始化与Agent的连接
   - 返回协议版本和服务器信息

2. **tools/list**
   - 列出Agent提供的所有工具
   - 包含工具名称、描述和输入Schema

3. **tools/call**
   - 调用指定的工具
   - 传递参数并返回执行结果

### 错误处理

标准的JSON-RPC 2.0错误码：
- `-32600`: 无效的请求
- `-32601`: 方法未找到
- `-32603`: 内部错误

## 扩展点

### 1. 创建自定义Agent

继承 `AgentBase` 类并实现：
- `GetToolsAsync()` - 定义工具
- `ExecuteToolAsync()` - 实现工具逻辑

### 2. 添加中间件

在 `Program.cs` 中添加ASP.NET Core中间件：
- 身份验证
- 授权
- 日志记录
- 限流

### 3. 集成外部服务

通过依赖注入集成：
- 数据库
- 缓存
- 消息队列
- 外部API

### 4. 自定义MCP方法

重写 `HandleRequestAsync()` 添加自定义方法处理。

## 安全考虑

1. **输入验证**
   - 验证所有工具参数
   - 使用JSON Schema定义输入格式

2. **身份验证和授权**
   - 添加API密钥验证
   - 实现基于角色的访问控制

3. **速率限制**
   - 防止滥用
   - 保护资源

4. **日志和监控**
   - 记录所有请求
   - 监控性能指标
   - 设置告警

## 性能优化

1. **异步处理**
   - 所有I/O操作使用async/await
   - 避免阻塞线程

2. **缓存**
   - 缓存工具列表
   - 缓存频繁访问的数据

3. **资源池化**
   - 复用HTTP客户端
   - 数据库连接池

4. **负载均衡**
   - 部署多个实例
   - 使用反向代理

## 部署架构

### 开发环境
```
开发机 → Kestrel (http://localhost:5000)
```

### 生产环境
```
Client → Load Balancer → [Nginx/IIS] → [ConstellationAI Instance 1]
                                     → [ConstellationAI Instance 2]
                                     → [ConstellationAI Instance N]
```

## 技术栈

- **运行时**: .NET 10.0
- **Web框架**: ASP.NET Core
- **AI框架**: Microsoft Semantic Kernel
- **协议**: MCP (Model Context Protocol)
- **序列化**: System.Text.Json

## 未来规划

1. **Agent间通信**
   - Agent可以互相调用
   - 工作流编排

2. **流式响应**
   - 支持Server-Sent Events
   - WebSocket支持

3. **Agent市场**
   - Agent发现和分享
   - 版本管理

4. **监控和分析**
   - 性能监控
   - 使用统计
   - 错误追踪

5. **配置管理**
   - 动态配置更新
   - 配置版本控制
