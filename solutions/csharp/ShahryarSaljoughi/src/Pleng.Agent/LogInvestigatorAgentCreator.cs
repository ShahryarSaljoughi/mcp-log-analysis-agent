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
                    Arguments = ["run", "--project", mcpPath]
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

internal class DataCollectorCreator
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
            BackendType.OpenAI => new OpenAIClientProvider(loggerFactory, options =>
            {
                options.ResponseFormat = ChatResponseFormat.ForJsonSchema(
                    AIJsonUtilities.CreateJsonSchema(typeof(DataCollectionResult)));
                options.Instructions = GetAgentInstructions();
            }),
            BackendType.Fake => new FakeLLMClientProvider(),
            _ => new FakeLLMClientProvider()
        };

        var agent = new ChatClientAgent(
            chatClientProvider.Create(agentConfig),
            instructions: GetAgentInstructions())
            .AsBuilder()
            .UseLogging(loggerFactory)
            .Build();

        return agent;
    }


    private string GetAgentInstructions()
    {
        return @"You assist with extract out needed data from user messages. 
Your response should be a json filled with appropriate values based on user data. 
If you any data is missing and not present in the user's messages, place it in the `MissingInputs` property. otherwise, MissingInputs should be an empty list.";
    }
}


internal class PlatformEngineer
{
    private AIAgent _logInvestigatorAgent;
    private AIAgent _dataCollectorAgent;

    public async Task InitializeAsync(Config agentConfig)
    {
        _logInvestigatorAgent = await new LogInvestigatorAgentCreator().CreateAsync(agentConfig);
    }

    Task<AgentResponse> RunAsync(string prompt)
    {
        throw new NotImplementedException();
    }
}

