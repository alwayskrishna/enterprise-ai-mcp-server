using IncidentMcpServer;
using Xunit;

namespace IncidentMcpServer.Tests;

public class RecoveryAuditTests
{
    [Fact]
    public void RejectedActionDoesNotCreatePlan()
    {
        var result = RecoveryWorkflow.Preview("INC-1001", "restart-real-service");
        Assert.Equal("Rejected", result.Status);
        Assert.Null(RecoveryWorkflow.GetPlan(result.PlanId));
    }

    [Fact]
    public void PlanIsBoundToIncidentAndAction()
    {
        var result = RecoveryWorkflow.Preview("INC-1001", "requeue-message-simulation");
        var plan = RecoveryWorkflow.GetPlan(result.PlanId);
        Assert.NotNull(plan);
        Assert.Equal("INC-1001", plan.IncidentId);
        Assert.Equal("requeue-message-simulation", plan.Action);
        Assert.Single(RecoveryWorkflow.GetAuditEvents(result.PlanId));
    }
}
