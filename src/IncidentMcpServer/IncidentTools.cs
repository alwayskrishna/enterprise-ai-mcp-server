using System.ComponentModel;
using ModelContextProtocol.Server;

namespace IncidentMcpServer;

public record Incident(string Id, string Service, string Severity, string Status, string Summary);

[McpServerToolType]
public static class IncidentTools
{
    private static readonly Incident[] Incidents =
    [
        new("INC-1001", "localization-worker", "SEV2", "Investigating", "Message processing latency exceeded threshold"),
        new("INC-1002", "document-ingestion", "SEV3", "Resolved", "Transient storage timeout")
    ];

    [McpServerTool, Description("Look up a synthetic incident by its incident ID.")]
    public static Incident? GetIncident([Description("Incident identifier, e.g. INC-1001")] string incidentId) =>
        Incidents.FirstOrDefault(i => string.Equals(i.Id, incidentId, StringComparison.OrdinalIgnoreCase));

    [McpServerTool, Description("List synthetic incidents. This is a read-only demo tool.")]
    public static Incident[] ListIncidents() => Incidents;

    [McpServerTool, Description("Return synthetic service health; no production systems are accessed.")]
    public static object GetServiceHealth([Description("Service name")] string serviceName)
    {
        var affected = Incidents.Any(i => i.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase)
            && i.Status != "Resolved");
        return new { Service = serviceName, Health = affected ? "Degraded" : "Healthy", Source = "synthetic-demo" };
    }
}
