# Pleng | Platform Engineering Agent




## How to Run
1. The agent, needs the following environment variables to be set before running:
- `BackendType`: The backend type to use for the agent. Valid values are `Fake` or `OpenAI`.
- `OPENAI_API_KEY`: The OpenAI API key to use for the agent. This is only required if `BackendType` is set to `OpenAI`.
- `OPENAI_ENDPOINT`: The OpenAI endpoint to use for the agent. This is only required if `BackendType` is set to `OpenAI`.  
You can either set these environment variables in your shell or use `launchSettings.json`.
2. Change to the `solutions\csharp\ShahryarSaljoughi` directory and run the agent using the following command:
```shell
dotnet run --project .\src\Pleng.Agent\ -lp Pleng.Agent-OpenAI -- "Why has the hub  been returning HTTP 500 errors during the last 15 minutes?"
```
Note that `lp Pleng.Agent-OpenAI` is used to specify the launch profile to use. You can also use `lp Pleng.Agent-Fake` to use the fake backend for testing purposes. In case of using OpenAI, Make sure you've placed your API key and Endpoint in the `launchSettings` file.

## Sample Execution Output

### using Fake LLM
```shell
> dotnet run --project .\src\Pleng.Agent\ -lp Pleng.Agent-Fake -- "Why has the hub  been returning HTTP 500 errors during the last 15 minutes?"
```
```shell
info: ModelContextProtocol.Client.StdioClientTransport[1587114612]
      Platform Engineer Agent | MCP Server connecting.

info: ModelContextProtocol.Client.StdioClientTransport[1355601408]
      Platform Engineer Agent | MCP Server started server process with PID 25256.

[ANSWER] I have gathered the relevant logs here: ```["[Error] - Payment - Database Connection Timedout after ms on SQL-Server-01"]```
```
### using OpenAI LLM
```shell
dotnet run --project .\src\Pleng.Agent\ -lp Pleng.Agent-OpenAI -- "Why has the hub  been returning HTTP 500 errors during the last 15 minutes?"
```
```shell
[ANSWER] Relevant logs: fetching logs for service name 'hub' failed (Service name is not valid). Available services are: Payment, Loan, CommunicationsHub — a logs pull for CommunicationsHub over the last 15 minutes returned no entries. Hints to fix: confirm the exact service name used by clients (the system has no plain “hub” — it may be CommunicationsHub), then check the ingress/load‑balancer and API‑gateway/nginx logs (those often return 500s when backend is unavailable); if the backend is the CommunicationsHub, verify pods/containers are running (kubectl get pods / docker ps), inspect pod/container logs directly (kubectl logs or docker logs), check for CrashLoopBackOff, OOMKilled or recent deployments, and ensure the logging agent/sidecar is healthy (Fluentd/Promtail) so logs are being forwarded; if no app logs exist, triage at the ingress level and restart the service instance(s) or roll back the last deploy while you gather stack traces and metrics (CPU/memory/restarts) to pinpoint the 500 source.
```