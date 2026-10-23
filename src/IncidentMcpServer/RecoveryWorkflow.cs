using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace IncidentMcpServer;

public record RecoveryPlan(string PlanId, string IncidentId, string Service, string Action, string Status, DateTimeOffset CreatedUtc);
public record RecoveryResult(string PlanId, string Status, string Message);
public record RecoveryAuditEvent(DateTimeOffset TimestampUtc, string PlanId, string Event, string Detail);

/// <summary>
/// In-memory, synthetic-only remediation workflow. No infrastructure adapters or external actions.
/// Approval tokens are never exposed to MCP tools: approvals require a separate trusted control plane.
/// </summary>
public static class RecoveryWorkflow
{
    private static readonly ConcurrentDictionary<string, RecoveryPlan> Plans = new();
    private static readonly ConcurrentQueue<RecoveryAuditEvent> Audit = new();
    private static readonly string[] AllowedActions = ["restart-worker-simulation", "requeue-message-simulation"];

    public static RecoveryResult Preview(string incidentId, string action)
    {
        var incident = IncidentTools.GetIncident(incidentId);
        if (incident is null) return new("", "Rejected", "Unknown incident.");
        if (incident.Status == "Resolved") return new("", "Rejected", "Incident is already resolved.");
        if (!AllowedActions.Contains(action, StringComparer.Ordinal))
            return new("", "Rejected", "Action is not on the simulation allowlist.");
        var id = Guid.NewGuid().ToString("N");
        var plan = new RecoveryPlan(id, incident.Id, incident.Service, action, "PendingApproval", DateTimeOffset.UtcNow);
        Plans[id] = plan;
        Audit.Enqueue(new(DateTimeOffset.UtcNow, id, "PreviewCreated", $"{incident.Id}: {action}"));
        return new(id, "PendingApproval", $"DRY RUN ONLY. Proposed simulation: {action} for {incident.Service}. Requires trusted approval before simulation.");
    }

    public static RecoveryPlan? GetPlan(string planId) =>
        Plans.TryGetValue(planId, out var plan) ? plan : null;

    public static RecoveryAuditEvent[] GetAuditEvents(string planId) =>
        Audit.Where(e => e.PlanId == planId).ToArray();

    // Intentionally internal: MCP clients cannot approve plans. Future approval must use an authenticated control plane.
    internal static RecoveryResult ApproveAndSimulate(string planId, string approver)
    {
        if (string.IsNullOrWhiteSpace(approver)) return new(planId, "Rejected", "Approver identity required.");
        if (!Plans.TryGetValue(planId, out var plan)) return new(planId, "Rejected", "Plan not found.");
        if (plan.Status != "PendingApproval") return new(planId, "Rejected", "Plan is not pending approval.");
        var updated = plan with { Status = "Simulated" };
        if (!Plans.TryUpdate(planId, updated, plan)) return new(planId, "Rejected", "Plan state changed; retry.");
        Audit.Enqueue(new(DateTimeOffset.UtcNow, planId, "SimulationApproved", $"Approved by {approver}; no real action executed."));
        return new(planId, "Simulated", "Simulation recorded. No production service was changed.");
    }
}
