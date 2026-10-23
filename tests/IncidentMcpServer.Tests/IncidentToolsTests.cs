using IncidentMcpServer;
using Xunit;

namespace IncidentMcpServer.Tests;

public class IncidentToolsTests
{
    [Fact]
    public void Lookup_ReturnsKnownIncident()
    {
        var incident = IncidentTools.GetIncident("inc-1001");
        Assert.NotNull(incident);
        Assert.Equal("localization-worker", incident.Service);
    }

    [Fact]
    public void Lookup_UnknownIdReturnsNull()
    {
        Assert.Null(IncidentTools.GetIncident("INC-9999"));
    }

    [Fact]
    public void List_ReturnsSyntheticIncidents()
    {
        Assert.Equal(2, IncidentTools.ListIncidents().Length);
    }
}
