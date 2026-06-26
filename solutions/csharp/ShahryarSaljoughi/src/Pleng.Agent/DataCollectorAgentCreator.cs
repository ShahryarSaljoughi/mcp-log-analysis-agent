using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Pleng.Agent;
using Pleng.Agent.LLMClients;

internal class DataCollectorAgentCreator
{
    public async Task<AIAgent> CreateAsync(Config agentConfig)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
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
            name: "Data Collector Agent",
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
The only required data are `ServiceName` and `MinutesAgo`.
If you find out any of those two required data is missing and not present in the user's messages, place it in the `MissingInputs` property. otherwise, MissingInputs should be an empty list.";
    }
}

