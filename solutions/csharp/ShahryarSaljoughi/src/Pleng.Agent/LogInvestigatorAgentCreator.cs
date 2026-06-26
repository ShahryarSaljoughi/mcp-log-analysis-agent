using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using Pleng.Agent;
using Pleng.Agent.LLMClients;
using System.Diagnostics;

internal class LogInvestigatorAgentCreator
{
    public async Task<AIAgent> CreateAsync(Config agentConfig)
    {
        var loggerFactory = LogUtils.Default;

        ChatClientProvider chatClientProvider = agentConfig.BackendType switch
        {
            BackendType.OpenAI => new OpenAIClientProvider(loggerFactory, options =>
            {
                options.ResponseFormat = ChatResponseFormat.ForJsonSchema(
                    AIJsonUtilities.CreateJsonSchema(typeof(FinalAnswer)));
            }),
            BackendType.Fake => new FakeLLMClientProvider(),
            _ => new FakeLLMClientProvider()
        };

        var agent = new ChatClientAgent(
            chatClientProvider.Create(agentConfig),
            instructions: GetAgentInstructions(),
            tools: await GetToolsAsync()
            )
            .AsBuilder()
            .UseLogging(loggerFactory)
            .Build();

        return agent;
    }

    private async Task<AITool[]> GetToolsAsync()
    {
        var agentBinPath = AppContext.BaseDirectory;
        var mcpPath = Path.Combine(agentBinPath, "../../../../Pleng.MCP.Server");
        System.Diagnostics.Debugger.Log((int)LogLevel.Information, "PlatformEngineerCreator", $"MCP Path: {mcpPath}");
        var mcpClient = await McpClient.CreateAsync(
            new StdioClientTransport(
                new StdioClientTransportOptions
                {
                    Name = "Platform Engineer Agent | MCP Server",
                    Command = "dotnet",
                    Arguments = ["run", "--project", mcpPath],
                }, LogUtils.Default));

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