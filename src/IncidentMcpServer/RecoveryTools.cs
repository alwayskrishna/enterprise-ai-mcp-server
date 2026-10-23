using System.ComponentModel;
using ModelContextProtocol.Server;

namespace IncidentMcpServer;

[McpServerToolType]
public static class RecoveryTools
{
    [McpServerTool, Description("Preview a synthetic incident recovery plan. Dry run only; never executes a service operation.")]
    public static RecoveryResult PreviewRecovery(
        [Description("Incident identifier")] string incidentId,
        [Description("Allowed simulation action: restart-worker-simulation or requeue-message-simulation")] string action) =>
        RecoveryWorkflow.Preview(incidentId, action);

    [McpServerTool, Description("Inspect the status of a synthetic recovery plan.")]
    public static RecoveryPlan? GetRecoveryPlan([Description("Plan identifier")] string planId) =>
        RecoveryWorkflow.GetPlan(planId);

    [McpServerTool, Description("Read in-memory audit events for a synthetic recovery plan.")]
    public static RecoveryAuditEvent[] GetRecoveryAudit([Description("Plan identifier")] string planId) =>
        RecoveryWorkflow.GetAuditEvents(planId);
}
