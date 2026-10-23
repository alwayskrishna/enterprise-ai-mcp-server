using IncidentMcpServer;
using Xunit;

namespace IncidentMcpServer.Tests;

public class RecoveryWorkflowTests
{
    [Fact]
    public void Preview_CreatesPendingPlanAndAuditEvent()
    {
        var result = RecoveryWorkflow.Preview("INC-1001", "restart-worker-simulation");
        Assert.Equal("PendingApproval", result.Status);
        Assert.NotEmpty(result.PlanId);
        var plan = RecoveryWorkflow.GetPlan(result.PlanId);
        Assert.NotNull(plan);
        Assert.Equal("PendingApproval", plan.Status);
        Assert.Contains(RecoveryWorkflow.GetAuditEvents(result.PlanId), e => e.Event == "PreviewCreated");
    }

    [Fact]
    public void Preview_RejectsUnlistedAction()
    {
        var result = RecoveryWorkflow.Preview("INC-1001", "delete-resource");
        Assert.Equal("Rejected", result.Status);
        Assert.Equal("", result.PlanId);
    }

    [Fact]
    public void Preview_RejectsResolvedIncident()
    {
        var result = RecoveryWorkflow.Preview("INC-1002", "requeue-message-simulation");
        Assert.Equal("Rejected", result.Status);
    }

    [Fact]
    public void Preview_RejectsUnknownIncident()
    {
        var result = RecoveryWorkflow.Preview("INC-9999", "restart-worker-simulation");
        Assert.Equal("Rejected", result.Status);
    }
}
