# Azure OpenAI integration

The `diagnose_incident` MCP tool provides **advisory, read-only** incident analysis. It sends only the selected synthetic incident record to an Azure OpenAI chat-completions deployment.

## Configuration

Set the following environment variables in the process launching the server:

- `AZURE_OPENAI_ENDPOINT` — HTTPS resource URL, e.g. `https://your-resource.openai.azure.com/`
- `AZURE_OPENAI_DEPLOYMENT` — deployed chat model name
- `AZURE_OPENAI_API_KEY` — secret API key; **never commit it**

The client subprocess inherits the environment. When variables are absent, the tool returns a configuration message rather than calling Azure.

## Guardrails and limitations

- Read-only: no Azure resource modification or remediation commands
- Hostname is restricted to Azure OpenAI resource domains
- Request timeouts and sanitized error messages
- Synthetic incident content only; no production telemetry
- API key authentication is suitable for a local demo, not a preferred production identity model
- Model output is untrusted advice; a human must verify it
- Azure OpenAI usage may incur charges; no calls are made without configuration

Next steps: managed identity, audit events, evaluation datasets, tests with mocked HTTP and approval-gated remediation.
