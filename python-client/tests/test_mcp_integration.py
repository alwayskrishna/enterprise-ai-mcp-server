"""Real end-to-end tests against the .NET MCP server over stdio."""
import asyncio
import os
import shutil
from pathlib import Path

import pytest
from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / "src" / "IncidentMcpServer" / "IncidentMcpServer.csproj"


async def invoke(name: str, arguments: dict):
    server = StdioServerParameters(
        command="dotnet",
        args=["run", "--no-build", "--project", str(PROJECT)],
        cwd=str(ROOT),
        env=os.environ.copy(),
    )
    async with stdio_client(server) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            tools = await session.list_tools()
            assert name in [tool.name for tool in tools.tools]
            return await session.call_tool(name, arguments)


@pytest.mark.skipif(shutil.which("dotnet") is None, reason=".NET SDK unavailable")
def test_incident_lookup_over_stdio():
    result = asyncio.run(invoke("get_incident", {"incidentId": "INC-1001"}))
    assert not result.isError
    assert "localization-worker" in str(result.content)


@pytest.mark.skipif(shutil.which("dotnet") is None, reason=".NET SDK unavailable")
def test_unknown_incident_over_stdio():
    result = asyncio.run(invoke("get_incident", {"incidentId": "INC-9999"}))
    assert not result.isError


@pytest.mark.skipif(shutil.which("dotnet") is None, reason=".NET SDK unavailable")
def test_recovery_preview_rejects_unsafe_action():
    result = asyncio.run(invoke("preview_recovery", {
        "incidentId": "INC-1001", "action": "delete-resource"
    }))
    assert not result.isError
    assert "Rejected" in str(result.content)
