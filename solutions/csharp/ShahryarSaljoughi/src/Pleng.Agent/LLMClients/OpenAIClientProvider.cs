using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Text;

namespace Pleng.Agent.LLMClients;

internal class OpenAIClientProvider(ILoggerFactory loggerFactory) : ChatClientProvider
{
    public override IChatClient Create(Config config)
    {
        var openAiClient = new OpenAIClient(
                new ApiKeyCredential(config.OpenAIApiKey!),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(config.OpenAIEndpointBase!),
                });


        var chatClient = openAiClient
            .GetChatClient("gpt-5-mini") // claude-haiku-4-5
            .AsIChatClient()
            .AsBuilder()
            .UseLogging(loggerFactory)
            .UseOpenTelemetry(loggerFactory, sourceName: "Pleng.Agent", c => c.EnableSensitiveData = true)
            .Build();

        return chatClient;
    }
}

