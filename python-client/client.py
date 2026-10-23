"""Run the .NET MCP server over stdio and query its read-only tools."""
import asyncio
import os

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client


async def main() -> None:
    server = StdioServerParameters(
        command="dotnet",
        args=["run", "--project", "src/IncidentMcpServer/IncidentMcpServer.csproj"],
        env=os.environ.copy(),
    )
    async with stdio_client(server) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            tools = await session.list_tools()
            print("Available tools:", [tool.name for tool in tools.tools])
            result = await session.call_tool("get_incident", {"incidentId": "INC-1001"})
            for item in result.content:
                if getattr(item, "text", None):
                    print(item.text)


if __name__ == "__main__":
    asyncio.run(main())
