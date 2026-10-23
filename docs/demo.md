# Demo walkthrough

This walkthrough uses only synthetic data. It does not connect to production systems or execute recovery actions.

## 1. Prerequisites

Install .NET 10 SDK and Python 3.12+. Run commands from the repository root.

```bash
dotnet restore tests/IncidentMcpServer.Tests/IncidentMcpServer.Tests.csproj
dotnet build src/IncidentMcpServer/IncidentMcpServer.csproj
dotnet test tests/IncidentMcpServer.Tests/IncidentMcpServer.Tests.csproj
python -m pip install -r python-client/requirements.txt
python -m pytest python-client/tests -q
```

## 2. Run the client

```bash
python python-client/client.py
```

The Python client starts the C# MCP server via stdio, discovers tools, and fetches incident `INC-1001`.

## 3. Available operations

| Tool | Purpose | Safety |
| --- | --- | --- |
| `get_incident` | Look up synthetic incident | Read-only |
| `list_incidents` | List synthetic incidents | Read-only |
| `get_service_health` | Show synthetic service health | Read-only |
| `diagnose_incident` | Advisory Azure OpenAI analysis | Optional credentials; read-only |
| `preview_recovery` | Create synthetic dry-run plan | No service mutation |
| `get_recovery_plan` | Inspect a plan | Read-only |
| `get_recovery_audit` | Inspect demo audit trail | Read-only |

## 4. Recovery guardrails

Try `preview_recovery` with `delete-resource`: it is rejected. Allowed actions are simulations only. The MCP server has no approval or production execution tool.

## 5. Known limitations

- Synthetic incidents and process-local recovery plans
- Audit trail is in-memory and not durable
- No production identity, real telemetry, or recovery executor
- Azure OpenAI requires explicit configuration and may incur usage charges
- No AI evaluation benchmark yet

See [architecture](architecture.md), [Azure OpenAI configuration](azure-openai.md), and [recovery workflow](recovery-workflows.md).
