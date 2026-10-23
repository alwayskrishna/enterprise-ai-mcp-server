using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace IncidentMcpServer;

/// <summary>Optional Azure OpenAI incident analysis. Never performs recovery actions.</summary>
public static class AzureOpenAiDiagnosis
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(30) };

    public static async Task<string> DiagnoseAsync(string incidentId, CancellationToken cancellationToken = default)
    {
        var incident = IncidentTools.GetIncident(incidentId);
        if (incident is null) return "Incident not found.";

        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(deployment) || string.IsNullOrWhiteSpace(apiKey))
            return "AI diagnosis is not configured. Set AZURE_OPENAI_ENDPOINT, AZURE_OPENAI_DEPLOYMENT, and AZURE_OPENAI_API_KEY.";

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri) || baseUri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(baseUri.UserInfo) || !string.IsNullOrEmpty(baseUri.Query) || !string.IsNullOrEmpty(baseUri.Fragment))
            return "AZURE_OPENAI_ENDPOINT must be an HTTPS Azure OpenAI resource URL.";

        if (!baseUri.Host.EndsWith(".openai.azure.com", StringComparison.OrdinalIgnoreCase))
            return "AZURE_OPENAI_ENDPOINT must use an Azure OpenAI resource hostname.";

        if (deployment.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            return "AZURE_OPENAI_DEPLOYMENT contains invalid characters.";

        var url = new Uri(baseUri, $"/openai/deployments/{Uri.EscapeDataString(deployment)}/chat/completions?api-version=2024-10-21");
        var prompt = $"Incident: {incident.Id}; Service: {incident.Service}; Severity: {incident.Severity}; Status: {incident.Status}; Summary: {incident.Summary}";
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-key", apiKey);
        request.Content = JsonContent.Create(new
        {
            messages = new object[]
            {
                new { role = "system", content = "You are a cautious incident analyst. Use only provided synthetic incident details. Provide possible causes, investigation steps, and confidence limitations. Do not claim to have accessed logs or executed actions. Never issue recovery commands." },
                new { role = "user", content = prompt }
            },
            temperature = 0.2,
            max_tokens = 400
        });
        try
        {
            using var response = await Http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return $"Azure OpenAI request failed (HTTP {(int)response.StatusCode}). Verify deployment, API access and configuration.";
            using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
            if (json.RootElement.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) && message.TryGetProperty("content", out var content))
                return content.GetString() ?? "Azure OpenAI returned an empty diagnosis.";
            return "Azure OpenAI response did not contain a diagnosis.";
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return "Azure OpenAI request timed out.";
        }
        catch (HttpRequestException)
        {
            return "Azure OpenAI network request failed. Verify connectivity and endpoint configuration.";
        }
    }
}
