using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Pleng.Agent;
using Pleng.Agent.LLMClients;

internal class PlatformEngineerCreator
{
    public async Task<AIAgent> CreateAsync(Config agentConfig)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddConsole(opt => { });
            builder.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });
        });

        ChatClientProvider chatClientProvider = agentConfig.BackendType switch
        {
            BackendType.OpenAI => new OpenAIClientProvider(loggerFactory),
            BackendType.Fake => new FakeLLMClientProvider(),
            _ => new FakeLLMClientProvider()
        };

        var agent = new ChatClientAgent(
            chatClientProvider.Create(agentConfig),
            instructions: GetAgentInstructions(),
            tools: await GetToolsAsync()
            );

        return agent;
    }

    private async Task<AITool[]> GetToolsAsync()
    {
        var mcpClient = await McpClient.CreateAsync(
            new StdioClientTransport(
                new StdioClientTransportOptions
                {
                    Name = "Platform Engineer Agent | MCP Server",
                    Command = "dotnet",
                    Arguments = ["run", "--project", "../../../../Pleng.MCP.Server"]
                }));

        var mcpTools = await mcpClient.ListToolsAsync();
        return [.. mcpTools.Cast<AITool>()];

    }

    private string GetAgentInstructions()
    {
        return @"You are a senior platform engineer.
You will be asked about the status of our infrastructure and how healthy they work.
You should guide us to fix issues in case logs show such.
You can use `get_log` tool for accessing logs.
Your answers should start by indicating relevant logs and followed by  at most a single paragraph giving hints on how to fix.";

    }
}

