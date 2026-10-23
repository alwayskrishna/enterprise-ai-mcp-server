# Evaluation strategy

## Current automated suite

`evals/run_evals.py` exercises the running .NET MCP server through the Python MCP client and evaluates deterministic tool responses against `evals/cases.json`.

The four fixtures cover:
1. Known incident retrieval
2. Missing incident behavior
3. Rejection of an unsafe recovery action
4. An allowlisted dry-run recovery preview

Run from the repository root:

```bash
python -m pip install -r python-client/requirements.txt
python evals/run_evals.py --output evals/results.json
```

A JSON report includes case outcomes, timestamp, and pass count. CI also uploads the report as an artifact.

## What these scores do **not** mean

These checks validate **deterministic MCP tool behavior**, not AI reasoning quality, diagnosis accuracy, production reliability, or Azure OpenAI model performance. They use synthetic data and do not call a language model.

## Future model evaluations

Build a separate opt-in evaluation suite using anonymized, labeled incident scenarios. Evaluate groundedness, suggested next steps, unsafe-action refusal, latency, and cost. Require human-reviewed ground truth, track model/deployment versions, and do not store secrets or sensitive logs in GitHub.
