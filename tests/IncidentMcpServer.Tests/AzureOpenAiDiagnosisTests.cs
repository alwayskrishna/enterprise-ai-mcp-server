using IncidentMcpServer;
using Xunit;

namespace IncidentMcpServer.Tests;

public class AzureOpenAiDiagnosisTests
{
    [Fact]
    public async Task UnknownIncidentDoesNotRequireConfiguration()
    {
        var result = await AzureOpenAiDiagnosis.DiagnoseAsync("INC-DOES-NOT-EXIST");
        Assert.Equal("Incident not found.", result);
    }
}
