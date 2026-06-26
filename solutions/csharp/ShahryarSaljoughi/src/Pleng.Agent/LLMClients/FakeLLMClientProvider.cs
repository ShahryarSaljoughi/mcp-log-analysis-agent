using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Pleng.Agent.LLMClients;

internal class FakeLLMClientProvider : ChatClientProvider
{
    public override IChatClient Create(Config config) => new FakeClient();

    private class FakeClient : IChatClient
    {
        public void Dispose() { }


        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (FakeClientHelper._currentStep == 0)
            {
                FakeClientHelper._currentStep++;
                return Task.FromResult(GetDataCollectionResponse());
            }
            else if (FakeClientHelper._currentStep == 1)
            {
                FakeClientHelper._currentStep++;
                return Task.FromResult(GetLogGatheringRequest());
            }

            return Task.FromResult(GetFinalAnswer(messages));



        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return this;
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = await GetResponseAsync(messages, options, cancellationToken);
            yield return new ChatResponseUpdate(ChatRole.Assistant, response.Text);
        }

        private ChatResponse GetLogGatheringRequest()
        {
            var response = new ChatResponse()
            {
                CreatedAt = DateTime.UtcNow,
                ResponseId = Guid.NewGuid().ToString(),
                Messages = [
                    new ChatMessage()
                {
                    CreatedAt = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Role = ChatRole.Assistant,
                    Contents = [
                        new FunctionCallContent(
                            Guid.NewGuid().ToString(),
                            "get_logs",
                            new Dictionary<string, object?>()
                            {
                                { "service_name", "Payment" } ,
                                { "minutes_ago", 60 }
                            })
                        ]
                }
                ]
            };
            return response;
        }

        private ChatResponse GetFinalAnswer(IEnumerable<ChatMessage> messages)
        {
            var toolCallResultContents = messages
                .Where(m => m.Contents.Any(c => c is FunctionResultContent))
                .ToList();
            var toolCallResponse = string.Join(Environment.NewLine, toolCallResultContents.Select(m => m.Text));
            var result = new FinalAnswer
            {
                FinalAnswerText = $"I have gathered the relevant logs here: ```{toolCallResponse}```",
                ErrorMessage = ""
            };
            var response = new ChatResponse()
            {
                CreatedAt = DateTime.UtcNow,
                ResponseId = Guid.NewGuid().ToString(),
                Messages = [
                    new ChatMessage()
                {
                    CreatedAt = DateTime.UtcNow,
                    MessageId = Guid.NewGuid().ToString(),
                    Role = ChatRole.Assistant,
                    Contents = [
                        new TextContent(JsonSerializer.Serialize<FinalAnswer>(
                            result,
                            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase}))
                    ]
                }
                ]
            };
            return response;
        }

        private ChatResponse GetDataCollectionResponse()
        {
            var result = new DataCollectionResult
            {
                MinutesAgo = 15,
                ServiceName = "Payment",
                MissingInputs = [],
                InitialUserMessage = "Please provide the logs for the last 15 minutes.",
            };

            var response = new ChatResponse()
            {
                CreatedAt = DateTime.UtcNow,
                ResponseId = Guid.NewGuid().ToString(),
                Messages = [
                    new ChatMessage()
                    {
                        CreatedAt = DateTime.UtcNow,
                        MessageId = Guid.NewGuid().ToString(),
                        Role = ChatRole.Assistant,
                        Contents = [
                            new TextContent(JsonSerializer.Serialize<DataCollectionResult>(
                                result,
                                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase}))
                            ]
                    }
                ]
            };
            return response;
        }
    }

    private static class FakeClientHelper
    {
        /// <summary>
        /// Every time GetResponseAsync is called, _currentStep increases by 1.
        /// In this fake client, we can use this to simulate the step of the conversation.
        /// </summary>
        internal static int _currentStep = 0;
    }
}


