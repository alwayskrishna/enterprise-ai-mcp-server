"""Deterministic MCP tool-behavior evaluations; no model scoring or Azure calls."""
import argparse
import asyncio
import json
import os
from datetime import datetime, timezone
from pathlib import Path

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client

ROOT = Path(__file__).resolve().parents[1]
CASES = ROOT / "evals" / "cases.json"
PROJECT = ROOT / "src" / "IncidentMcpServer" / "IncidentMcpServer.csproj"


async def evaluate():
    cases = json.loads(CASES.read_text(encoding="utf-8"))
    server = StdioServerParameters(
        command="dotnet",
        args=["run", "--project", str(PROJECT)],
        cwd=str(ROOT),
        env=os.environ.copy(),
    )
    results = []
    async with stdio_client(server) as (read, write):
        async with ClientSession(read, write) as session:
            await session.initialize()
            for case in cases:
                args = {"incidentId": case["incident_id"], **case.get("arguments", {})}
                try:
                    response = await session.call_tool(case["expected_tool"], args)
                    output = " ".join(getattr(item, "text", "") for item in response.content)
                    passed = not response.is_error and case["expected_text"].lower() in output.lower()
                    results.append({
                        "id": case["id"],
                        "passed": passed,
                        "tool": case["expected_tool"],
                        "error": bool(response.is_error),
                    })
                except Exception as exc:
                    results.append({
                        "id": case["id"], "passed": False,
                        "tool": case["expected_tool"], "error": type(exc).__name__,
                    })
    return results


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", default="evals/results.json")
    args = parser.parse_args()
    results = asyncio.run(evaluate())
    report = {
        "evaluation_type": "deterministic-mcp-tool-behavior",
        "timestamp_utc": datetime.now(timezone.utc).isoformat(),
        "total": len(results),
        "passed": sum(item["passed"] for item in results),
        "results": results,
    }
    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    print(f"Passed {report['passed']}/{report['total']} deterministic MCP cases. Report: {output}")
    raise SystemExit(0 if report["passed"] == report["total"] else 1)


if __name__ == "__main__":
    main()
