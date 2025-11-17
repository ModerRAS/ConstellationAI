# MCP服务器测试脚本 (PowerShell)
# 用法: .\test-mcp.ps1 [-ServerUrl "http://localhost:5000"]

param(
    [string]$ServerUrl = "http://localhost:5000"
)

$AgentPath = "/agents/echo"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "ConstellationAI MCP 服务器测试" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host "服务器: $ServerUrl"
Write-Host ""

# 测试函数
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method,
        [string]$Url,
        [string]$Body
    )
    
    Write-Host "测试: $Name" -ForegroundColor Blue
    Write-Host "请求: $Method $Url"
    if ($Body) {
        Write-Host "数据: $Body"
    }
    Write-Host ""
    
    try {
        $response = if ($Body) {
            Invoke-RestMethod -Uri $Url -Method $Method -ContentType "application/json" -Body $Body
        } else {
            Invoke-RestMethod -Uri $Url -Method $Method
        }
        
        Write-Host "✓ 成功" -ForegroundColor Green
        $response | ConvertTo-Json -Depth 10
    }
    catch {
        Write-Host "✗ 失败" -ForegroundColor Red
        Write-Host $_.Exception.Message -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "-----------------------------------"
    Write-Host ""
}

# 测试根端点
Test-Endpoint -Name "服务器信息" -Method "GET" -Url "$ServerUrl/"

# 测试MCP协议
Test-Endpoint -Name "初始化连接" -Method "POST" -Url "$ServerUrl$AgentPath" -Body @'
{
    "jsonrpc": "2.0",
    "id": "1",
    "method": "initialize",
    "params": {}
}
'@

Test-Endpoint -Name "列出工具" -Method "POST" -Url "$ServerUrl$AgentPath" -Body @'
{
    "jsonrpc": "2.0",
    "id": "2",
    "method": "tools/list"
}
'@

Test-Endpoint -Name "调用echo工具" -Method "POST" -Url "$ServerUrl$AgentPath" -Body @'
{
    "jsonrpc": "2.0",
    "id": "3",
    "method": "tools/call",
    "params": {
        "name": "echo",
        "arguments": {
            "message": "Hello, ConstellationAI!"
        }
    }
}
'@

Test-Endpoint -Name "调用reverse工具" -Method "POST" -Url "$ServerUrl$AgentPath" -Body @'
{
    "jsonrpc": "2.0",
    "id": "4",
    "method": "tools/call",
    "params": {
        "name": "reverse",
        "arguments": {
            "text": "ConstellationAI"
        }
    }
}
'@

Write-Host "================================" -ForegroundColor Cyan
Write-Host "测试完成" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
