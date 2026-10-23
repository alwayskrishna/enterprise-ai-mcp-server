# Enterprise AI MCP Server

A portfolio project demonstrating Model Context Protocol (MCP) with a .NET 10 C# server and a Python 3.12 client.

**Status: v1 scaffold.** Synthetic incidents only. AI diagnosis, Azure OpenAI, production adapters and recovery actions are planned, not yet implemented.

## Features
- Read-only MCP incident lookup, incident listing and service health tools
- Python client using MCP stdio transport
- GitHub Actions build checks
- Architecture and security notes in [docs/architecture.md](docs/architecture.md)

## Prerequisites
.NET 10 SDK and Python 3.12 or newer.

## Run from the repository root

```bash
dotnet restore src/IncidentMcpServer/IncidentMcpServer.csproj
dotnet build src/IncidentMcpServer/IncidentMcpServer.csproj
python -m venv .venv
# Linux/macOS: source .venv/bin/activate
# Windows PowerShell: .venv\Scripts\Activate.ps1
python -m pip install -r python-client/requirements.txt
python python-client/client.py
```

The Python client starts the .NET MCP server and queries incident INC-1001.

## Roadmap
1. Add xUnit and pytest integration tests.
2. Add Azure OpenAI diagnosis and tool selection.
3. Add authenticated Azure Monitor adapters.
4. Add evaluation, audit logging and approval-gated recovery.

No real service mutations or recovery operations are implemented. Never commit secrets or proprietary incident data.
