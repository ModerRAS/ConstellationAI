#!/bin/bash
# MCP服务器测试脚本
# 用法: ./test-mcp.sh [服务器URL]

SERVER_URL="${1:-http://localhost:5000}"
AGENT_PATH="/agents/echo"

echo "================================"
echo "ConstellationAI MCP 服务器测试"
echo "================================"
echo "服务器: $SERVER_URL"
echo ""

# 颜色定义
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 测试函数
test_endpoint() {
    local name=$1
    local method=$2
    local data=$3
    
    echo -e "${BLUE}测试: $name${NC}"
    echo "请求: $method $SERVER_URL$AGENT_PATH"
    echo "数据: $data"
    echo ""
    
    response=$(curl -s -X "$method" "$SERVER_URL$AGENT_PATH" \
        -H "Content-Type: application/json" \
        -d "$data")
    
    if [ $? -eq 0 ]; then
        echo -e "${GREEN}✓ 成功${NC}"
        echo "$response" | python3 -m json.tool 2>/dev/null || echo "$response"
    else
        echo -e "${RED}✗ 失败${NC}"
    fi
    echo ""
    echo "-----------------------------------"
    echo ""
}

# 测试根端点
echo -e "${BLUE}测试: 服务器信息${NC}"
echo "请求: GET $SERVER_URL/"
echo ""
response=$(curl -s "$SERVER_URL/")
if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ 成功${NC}"
    echo "$response" | python3 -m json.tool 2>/dev/null || echo "$response"
else
    echo -e "${RED}✗ 失败${NC}"
fi
echo ""
echo "-----------------------------------"
echo ""

# 测试MCP协议
test_endpoint "初始化连接" "POST" '{
    "jsonrpc": "2.0",
    "id": "1",
    "method": "initialize",
    "params": {}
}'

test_endpoint "列出工具" "POST" '{
    "jsonrpc": "2.0",
    "id": "2",
    "method": "tools/list"
}'

test_endpoint "调用echo工具" "POST" '{
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

test_endpoint "调用reverse工具" "POST" '{
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

echo "================================"
echo "测试完成"
echo "================================"
