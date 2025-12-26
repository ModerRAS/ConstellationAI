# ConstellationAI
未完成的项目，不过也不想写了。
基于Semantic Kernel的MCP（Model Context Protocol）服务器框架

## 项目简介

ConstellationAI是一个使用C#和Semantic Kernel构建的MCP服务器项目。它允许你在不同的URL路径下部署多个智能体（Agent），每个Agent都可以作为MCP服务器被LLM调用。

## 项目架构

```
ConstellationAI/
├── src/
│   ├── ConstellationAI.Core/          # 核心库
│   │   ├── Models/                     # MCP协议模型
│   │   │   ├── McpRequest.cs          # MCP请求
│   │   │   ├── McpResponse.cs         # MCP响应
│   │   │   └── ToolInfo.cs            # 工具信息
│   │   ├── Agents/                     # Agent定义
│   │   │   ├── IAgent.cs              # Agent接口
│   │   │   ├── AgentBase.cs           # Agent基类
│   │   │   └── Examples/              # 示例Agent
│   │   │       └── EchoAgent.cs       # 回声Agent示例
│   │   └── Services/                   # 服务
│   │       ├── IAgentRegistry.cs      # Agent注册表接口
│   │       └── AgentRegistry.cs       # Agent注册表实现
│   └── ConstellationAI.Server/         # Web服务器
│       └── Program.cs                  # 启动程序
└── ConstellationAI.sln                 # 解决方案文件
```

## 核心概念

### Agent（智能体）

Agent是MCP服务器的核心组件，每个Agent：
- 有唯一的ID和URL路径
- 提供一组工具（Tools）供LLM调用
- 实现MCP协议处理逻辑
- 可以集成Semantic Kernel进行AI能力增强

### MCP协议

项目实现了标准的MCP（Model Context Protocol）协议，支持：
- `initialize` - 初始化连接
- `tools/list` - 列出可用工具
- `tools/call` - 调用工具

### Agent注册

通过`IAgentRegistry`服务管理所有Agent：
- 支持按URL路径或ID查找Agent
- 防止重复注册
- 自动路径标准化

## 快速开始

### 前置要求

- .NET 10.0 SDK
- Visual Studio 2022 或 VS Code

### 构建和运行

```bash
# 克隆仓库
git clone https://github.com/ModerRAS/ConstellationAI.git
cd ConstellationAI

# 构建项目
dotnet build

# 运行服务器
cd src/ConstellationAI.Server
dotnet run
```

服务器默认运行在 `http://localhost:5000` 或 `https://localhost:5001`

### 测试MCP端点

1. 获取服务器信息：
```bash
curl http://localhost:5000/
```

2. 初始化Agent连接：
```bash
curl -X POST http://localhost:5000/agents/echo \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "1",
    "method": "initialize",
    "params": {}
  }'
```

3. 列出工具：
```bash
curl -X POST http://localhost:5000/agents/echo \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "2",
    "method": "tools/list"
  }'
```

4. 调用工具：
```bash
curl -X POST http://localhost:5000/agents/echo \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "3",
    "method": "tools/call",
    "params": {
      "name": "echo",
      "arguments": {
        "message": "Hello, ConstellationAI!"
      }
    }
  }'
```

## 创建自定义Agent

### 1. 继承AgentBase类

```csharp
using ConstellationAI.Core.Agents;
using ConstellationAI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace YourNamespace;

public class MyCustomAgent : AgentBase
{
    public MyCustomAgent(ILogger<MyCustomAgent> logger, Kernel? kernel = null) 
        : base(logger, kernel)
    {
    }

    public override string Id => "my-custom-agent";
    public override string Name => "My Custom Agent";
    public override string Description => "我的自定义Agent";
    public override string UrlPath => "/agents/my-custom";

    public override Task<IEnumerable<ToolInfo>> GetToolsAsync()
    {
        // 定义你的工具
        var tools = new List<ToolInfo>
        {
            new ToolInfo
            {
                Name = "my_tool",
                Description = "我的工具描述",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        param1 = new { type = "string", description = "参数1" }
                    },
                    required = new[] { "param1" }
                }
            }
        };
        return Task.FromResult<IEnumerable<ToolInfo>>(tools);
    }

    public override Task<object> ExecuteToolAsync(string toolName, object? parameters)
    {
        // 实现你的工具逻辑
        return toolName switch
        {
            "my_tool" => ExecuteMyTool(parameters),
            _ => throw new NotSupportedException($"不支持的工具: {toolName}")
        };
    }

    private Task<object> ExecuteMyTool(object? parameters)
    {
        // 工具实现
        return Task.FromResult<object>(new { result = "success" });
    }
}
```

### 2. 注册Agent

在 `Program.cs` 中注册你的Agent：

```csharp
// 注册自定义Agent
builder.Services.AddSingleton<IAgent, MyCustomAgent>();
```

## 集成Semantic Kernel

框架已经内置Semantic Kernel支持。要使用AI功能，在`Program.cs`中配置：

```csharp
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    
    // 使用Azure OpenAI
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: "your-deployment",
        endpoint: "your-endpoint",
        apiKey: "your-api-key"
    );
    
    // 或使用OpenAI
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: "gpt-4",
        apiKey: "your-api-key"
    );
    
    return kernelBuilder.Build();
});
```

然后在你的Agent中使用`SemanticKernel`属性访问AI功能。

## 技术栈

- **ASP.NET Core** - Web服务器框架
- **Semantic Kernel** - AI编排和集成
- **C# 13** / **.NET 10.0** - 编程语言和运行时

## 许可证

本项目采用MIT许可证。详见 [LICENSE](LICENSE) 文件。

## 贡献

欢迎贡献！请随时提交Issue或Pull Request。

## 下一步计划

- [ ] 添加身份验证和授权
- [ ] 实现Agent之间的通信
- [ ] 添加更多示例Agent
- [ ] 支持流式响应
- [ ] 添加监控和日志记录
- [ ] 实现Agent配置管理
