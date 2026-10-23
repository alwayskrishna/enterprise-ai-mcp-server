using System.ComponentModel;
using ModelContextProtocol.Server;

namespace IncidentMcpServer;

[McpServerToolType]
public static class DiagnosisTools
{
    [McpServerTool, Description("Generate an advisory AI diagnosis for a synthetic incident. Read-only; does not perform recovery.")]
    public static Task<string> DiagnoseIncident(
        [Description("Incident identifier, e.g. INC-1001")] string incidentId,
        CancellationToken cancellationToken = default) =>
        AzureOpenAiDiagnosis.DiagnoseAsync(incidentId, cancellationToken);
}
