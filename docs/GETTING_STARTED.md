# 快速开始指南

本指南将帮助你快速搭建和运行ConstellationAI MCP服务器。

## 环境准备

### 系统要求
- .NET 10.0 SDK 或更高版本
- 任意文本编辑器或IDE（推荐Visual Studio 2022或VS Code）

### 安装.NET SDK

访问 [dotnet.microsoft.com](https://dotnet.microsoft.com/download) 下载并安装.NET SDK。

验证安装：
```bash
dotnet --version
```

## 克隆项目

```bash
git clone https://github.com/ModerRAS/ConstellationAI.git
cd ConstellationAI
```

## 构建项目

```bash
# 恢复依赖
dotnet restore

# 构建项目
dotnet build

# 验证构建成功
# 输出应该显示 "Build succeeded"
```

## 运行服务器

```bash
cd src/ConstellationAI.Server
dotnet run
```

服务器将启动在：
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## 测试服务器

### 1. 检查服务器状态

打开浏览器访问 `http://localhost:5000/` 或使用curl：

```bash
curl http://localhost:5000/
```

应该返回服务器信息和已注册的Agent列表。

### 2. 测试Echo Agent

Echo Agent是一个示例Agent，位于 `/agents/echo` 路径。

#### 初始化连接

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

预期响应：
```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "result": {
    "protocolVersion": "1.0",
    "serverInfo": {
      "name": "Echo Agent",
      "version": "1.0.0"
    },
    "capabilities": {
      "tools": {}
    }
  }
}
```

#### 列出可用工具

```bash
curl -X POST http://localhost:5000/agents/echo \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "2",
    "method": "tools/list"
  }'
```

预期响应将列出 `echo` 和 `reverse` 两个工具。

#### 调用echo工具

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

#### 调用reverse工具

```bash
curl -X POST http://localhost:5000/agents/echo \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "4",
    "method": "tools/call",
    "params": {
      "name": "reverse",
      "arguments": {
        "text": "ConstellationAI"
      }
    }
  }'
```

## 项目结构说明

```
ConstellationAI/
├── src/
│   ├── ConstellationAI.Core/          # 核心功能库
│   │   ├── Models/                     # 数据模型
│   │   ├── Agents/                     # Agent定义
│   │   └── Services/                   # 服务接口和实现
│   └── ConstellationAI.Server/         # Web服务器
│       └── Program.cs                  # 主入口
├── docs/                               # 文档
└── README.md                           # 项目说明
```

## 下一步

- [创建自定义Agent](CUSTOM_AGENT.md)
- [集成Semantic Kernel](SEMANTIC_KERNEL.md)
- [部署到生产环境](DEPLOYMENT.md)

## 常见问题

### 端口已被占用

如果默认端口被占用，可以通过命令行参数指定其他端口：

```bash
dotnet run --urls "http://localhost:5050;https://localhost:5051"
```

或修改 `appsettings.json` 文件：

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5050"
      }
    }
  }
}
```

### 构建失败

确保已安装.NET 10.0 SDK：
```bash
dotnet --version
```

清理并重新构建：
```bash
dotnet clean
dotnet restore
dotnet build
```

## 获取帮助

如遇到问题，请：
1. 查看项目[README](../README.md)
2. 查看[示例代码](../src/ConstellationAI.Core/Agents/Examples/)
3. 提交[Issue](https://github.com/ModerRAS/ConstellationAI/issues)
