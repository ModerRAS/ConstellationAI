# 创建自定义Agent指南

本指南将详细介绍如何创建和部署自定义Agent。

## Agent基础概念

每个Agent是一个独立的MCP服务器，提供特定的功能和工具。Agent可以：
- 响应MCP协议请求
- 提供多个工具（Tools）供LLM调用
- 使用Semantic Kernel进行AI能力增强
- 独立运行在自己的URL路径上

## 创建步骤

### 1. 创建Agent类

在 `src/ConstellationAI.Core/Agents/` 目录下创建新的Agent类，继承 `AgentBase`：

```csharp
using ConstellationAI.Core.Agents;
using ConstellationAI.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace ConstellationAI.Core.Agents;

public class CalculatorAgent : AgentBase
{
    public CalculatorAgent(ILogger<CalculatorAgent> logger, Kernel? kernel = null) 
        : base(logger, kernel)
    {
    }

    // Agent的唯一标识符
    public override string Id => "calculator-agent";
    
    // Agent的显示名称
    public override string Name => "Calculator Agent";
    
    // Agent的描述
    public override string Description => "提供基本数学计算功能的Agent";
    
    // Agent的URL路径
    public override string UrlPath => "/agents/calculator";

    // 定义Agent提供的工具
    public override Task<IEnumerable<ToolInfo>> GetToolsAsync()
    {
        var tools = new List<ToolInfo>
        {
            new ToolInfo
            {
                Name = "add",
                Description = "两数相加",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        a = new { type = "number", description = "第一个数" },
                        b = new { type = "number", description = "第二个数" }
                    },
                    required = new[] { "a", "b" }
                }
            },
            new ToolInfo
            {
                Name = "multiply",
                Description = "两数相乘",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        a = new { type = "number", description = "第一个数" },
                        b = new { type = "number", description = "第二个数" }
                    },
                    required = new[] { "a", "b" }
                }
            }
        };

        return Task.FromResult<IEnumerable<ToolInfo>>(tools);
    }

    // 执行工具调用
    public override Task<object> ExecuteToolAsync(string toolName, object? parameters)
    {
        Logger.LogInformation("执行工具: {ToolName}", toolName);

        return toolName.ToLower() switch
        {
            "add" => ExecuteAddAsync(parameters),
            "multiply" => ExecuteMultiplyAsync(parameters),
            _ => throw new NotSupportedException($"不支持的工具: {toolName}")
        };
    }

    private Task<object> ExecuteAddAsync(object? parameters)
    {
        var (a, b) = ParseTwoNumbers(parameters);
        var result = a + b;
        
        Logger.LogInformation("计算: {A} + {B} = {Result}", a, b, result);
        
        return Task.FromResult<object>(new { result });
    }

    private Task<object> ExecuteMultiplyAsync(object? parameters)
    {
        var (a, b) = ParseTwoNumbers(parameters);
        var result = a * b;
        
        Logger.LogInformation("计算: {A} * {B} = {Result}", a, b, result);
        
        return Task.FromResult<object>(new { result });
    }

    private (double a, double b) ParseTwoNumbers(object? parameters)
    {
        if (parameters == null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        var paramsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
            System.Text.Json.JsonSerializer.Serialize(parameters));

        if (paramsDict == null)
        {
            throw new ArgumentException("无效的参数");
        }

        double a = Convert.ToDouble(paramsDict["a"]);
        double b = Convert.ToDouble(paramsDict["b"]);

        return (a, b);
    }
}
```

### 2. 注册Agent

在 `src/ConstellationAI.Server/Program.cs` 中注册你的Agent：

```csharp
// 注册自定义Agent
builder.Services.AddSingleton<IAgent, CalculatorAgent>();
```

### 3. 测试Agent

重新构建并运行服务器：

```bash
dotnet build
cd src/ConstellationAI.Server
dotnet run
```

测试新的Agent：

```bash
# 列出工具
curl -X POST http://localhost:5000/agents/calculator \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "1",
    "method": "tools/list"
  }'

# 调用add工具
curl -X POST http://localhost:5000/agents/calculator \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "2",
    "method": "tools/call",
    "params": {
      "name": "add",
      "arguments": {
        "a": 10,
        "b": 20
      }
    }
  }'
```

## 高级功能

### 使用Semantic Kernel

如果你的Agent需要AI能力，可以使用 `SemanticKernel` 属性：

```csharp
public class SmartAgent : AgentBase
{
    public SmartAgent(ILogger<SmartAgent> logger, Kernel kernel) 
        : base(logger, kernel)
    {
    }

    public override async Task<object> ExecuteToolAsync(string toolName, object? parameters)
    {
        if (toolName == "analyze")
        {
            // 使用Semantic Kernel进行AI处理
            var result = await SemanticKernel!.InvokePromptAsync(
                "Analyze the following: {{$input}}",
                new KernelArguments { ["input"] = parameters }
            );
            
            return new { analysis = result.ToString() };
        }
        
        return await base.ExecuteToolAsync(toolName, parameters);
    }
}
```

### 添加依赖注入

如果Agent需要其他服务，可以通过构造函数注入：

```csharp
public class DataAgent : AgentBase
{
    private readonly IDataService _dataService;

    public DataAgent(
        ILogger<DataAgent> logger, 
        Kernel? kernel,
        IDataService dataService) 
        : base(logger, kernel)
    {
        _dataService = dataService;
    }

    // ... 使用 _dataService
}
```

然后在 `Program.cs` 中注册依赖：

```csharp
builder.Services.AddSingleton<IDataService, MyDataService>();
builder.Services.AddSingleton<IAgent, DataAgent>();
```

### 异步工具执行

对于长时间运行的操作，使用异步方法：

```csharp
private async Task<object> ExecuteLongRunningTaskAsync(object? parameters)
{
    Logger.LogInformation("开始长时间运行的任务");
    
    // 模拟长时间操作
    await Task.Delay(5000);
    
    Logger.LogInformation("任务完成");
    
    return new { status = "completed" };
}
```

### 错误处理

Agent基类已经提供了基本的错误处理，但你可以添加自定义错误处理：

```csharp
public override async Task<object> ExecuteToolAsync(string toolName, object? parameters)
{
    try
    {
        // 工具逻辑
        return await base.ExecuteToolAsync(toolName, parameters);
    }
    catch (ArgumentException ex)
    {
        Logger.LogWarning(ex, "参数错误");
        throw new InvalidOperationException("请提供有效的参数", ex);
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "执行工具时发生未知错误");
        throw;
    }
}
```

## 最佳实践

1. **命名规范**
   - Agent ID: 使用小写和连字符（例如：`my-agent`）
   - URL路径: 使用 `/agents/` 前缀（例如：`/agents/my-agent`）
   - 工具名: 使用小写和下划线（例如：`my_tool`）

2. **日志记录**
   - 记录所有重要操作
   - 使用适当的日志级别（Information、Warning、Error）
   - 包含足够的上下文信息

3. **输入验证**
   - 始终验证工具参数
   - 提供清晰的错误消息
   - 使用JSON Schema定义输入格式

4. **性能考虑**
   - 避免在工具执行中进行阻塞操作
   - 使用异步方法处理I/O操作
   - 考虑添加超时和取消支持

5. **测试**
   - 为每个工具编写单元测试
   - 测试边界情况和错误场景
   - 使用集成测试验证MCP协议交互

## 示例Agent

查看以下示例了解更多：
- [EchoAgent](../src/ConstellationAI.Core/Agents/Examples/EchoAgent.cs) - 基本的回声Agent

## 下一步

- [集成Semantic Kernel](SEMANTIC_KERNEL.md)
- [Agent间通信](AGENT_COMMUNICATION.md)
- [部署指南](DEPLOYMENT.md)
