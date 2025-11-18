using ConstellationAI.Core.Agents;
using ConstellationAI.Core.Agents.Examples;
using ConstellationAI.Core.Models;
using ConstellationAI.Core.Services;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// 注册Semantic Kernel
builder.Services.AddSingleton<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    // 这里可以添加AI服务配置，例如：
    // kernelBuilder.AddAzureOpenAIChatCompletion(...);
    // kernelBuilder.AddOpenAIChatCompletion(...);
    return kernelBuilder.Build();
});

// 注册Agent注册表
builder.Services.AddSingleton<IAgentRegistry, AgentRegistry>();

// 注册示例Agent
builder.Services.AddSingleton<IAgent, EchoAgent>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 初始化Agent注册表
var agentRegistry = app.Services.GetRequiredService<IAgentRegistry>();
var agents = app.Services.GetServices<IAgent>();

foreach (var agent in agents)
{
    agentRegistry.RegisterAgent(agent);
    app.Logger.LogInformation("已注册Agent: {Name} 在路径 {Path}", agent.Name, agent.UrlPath);
}

// MCP服务器根端点
app.MapGet("/", () => new
{
    name = "ConstellationAI MCP Server",
    version = "1.0.0",
    description = "基于Semantic Kernel的MCP服务器",
    agents = agentRegistry.GetAllAgents().Select(a => new
    {
        id = a.Id,
        name = a.Name,
        description = a.Description,
        path = a.UrlPath
    })
})
.WithName("GetServerInfo");

// MCP协议端点 - 处理所有Agent请求
app.MapPost("/{*path}", async (HttpContext context, string path, IAgentRegistry registry) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // 读取请求体
        var request = await context.Request.ReadFromJsonAsync<McpRequest>();
        if (request == null)
        {
            return Results.BadRequest(new McpResponse
            {
                Error = new McpError
                {
                    Code = -32600,
                    Message = "无效的请求"
                }
            });
        }

        // 标准化路径
        var normalizedPath = "/" + path.TrimStart('/');
        
        // 查找对应的Agent
        var agent = registry.GetAgentByPath(normalizedPath);
        if (agent == null)
        {
            logger.LogWarning("未找到Agent: {Path}", normalizedPath);
            return Results.NotFound(new McpResponse
            {
                Id = request.Id,
                Error = new McpError
                {
                    Code = -32601,
                    Message = $"未找到Agent: {normalizedPath}"
                }
            });
        }

        // 处理请求
        var response = await agent.HandleRequestAsync(request);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "处理MCP请求时发生错误: {Path}", path);
        return Results.Json(new McpResponse
        {
            Error = new McpError
            {
                Code = -32603,
                Message = "内部服务器错误",
                Data = ex.Message
            }
        }, statusCode: 500);
    }
})
.WithName("HandleMcpRequest");

app.Run();
