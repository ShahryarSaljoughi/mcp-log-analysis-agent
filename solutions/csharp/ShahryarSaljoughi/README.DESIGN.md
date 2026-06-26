Pleng — MCP Log Analysis Agent
==============================

Overview
--------
Pleng is a focused .NET 10 log collection and analysis solution composed of a lightweight Agent and an MCP Server. The Agent selects tools, retrieves application logs and asks the server for data; the Server provides discoverable tools (for example `GetLogs`), an ingestion/processing pipeline and utilities to explore analysis results.

Architecture (text diagram)
---------------------------

User -> Agent (Console) -> Tool Selection / Model Abstraction -> MCP Client -> MCP Server -> Tool Implementations -> Storage / Analysis -> Tools/UI

Components and responsibilities
-------------------------------
- src/Pleng.Agent
  - Agent runtime (Program.cs): starts the Agent, loads configuration and model/provider abstractions.
  - Tool selector / Model adapter: decides which tool to call and builds tool-call arguments (mock or real LLM behind an interface).
  - Transport: sends tool calls to the MCP Server (configurable transport).

- src/Pleng.MCP.Server
  - Ingestion and MCP host: registers tools and accepts tool calls from Agents.
  - Tool implementations: e.g. `GetLogs` that returns simulated log data for a given service/time range.
  - Processing pipeline: validation, enrichment and analysis of tool inputs/results.
  - Utilities: helper tools such as LogExplorerTools for inspecting stored or simulated logs.

Design decisions
----------------
- Separation of concerns: the Agent is responsible for deciding which tool to use and producing a tool call; the Server only executes tools and returns results.
- Model abstraction: the AI/decision logic must sit behind an interface so the Mock model can be replaced with a real LLM without changing the Agent core.
- Configuration-driven behavior: endpoints, transports, batching and retention must be configurable via appsettings or environment variables.
- Testability: simulated log providers and mock models make the solution runnable offline and suitable for automated tests.

How the pieces interact
-----------------------
1. User asks a question through the Agent console.
2. Agent queries its model adapter to select a tool and produce arguments (e.g., `GetLogs`, `{serviceName: "PaymentService", minutesAgo: 15}`).
3. The Agent sends a tool call to the MCP Server via the configured transport.
4. Server executes the tool, returning simulated logs or structured results.
5. Agent analyzes results and returns a clear, human-readable root-cause explanation.

Getting started (short)
------------------------
- Build: dotnet restore && dotnet build
- Run server: dotnet run --project src/Pleng.MCP.Server
- Run agent: dotnet run --project src/Pleng.Agent

Key files
---------
- src/Pleng.Agent/Program.cs — Agent entry point
- src/Pleng.MCP.Server/Pleng.MCP.Server.csproj — Server project
- src/Pleng.MCP.Server/Tools/LogExplorerTools.cs — Log exploration utilities

Extending the project
---------------------
- Add collectors to the Agent to support additional log sources.
- Add new tools on the Server (register them in the MCP host) and expose them to Agents.
- Implement additional storage adapters or processors for richer analysis and retention.

Notes
-----
- Target framework: .NET 10
- The design prioritizes replaceable model providers and offline testability.

License & contribution
----------------------
- Add a LICENSE at the repository root and open issues/PRs for contributions.

Contact
-------
Open an issue or create a pull request in the repository for questions or contributions.
