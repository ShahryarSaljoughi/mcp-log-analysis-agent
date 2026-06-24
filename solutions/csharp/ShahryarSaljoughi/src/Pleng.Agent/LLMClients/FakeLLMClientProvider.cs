using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;

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
            var isLogAvailable = messages.SelectMany(m => m.Contents)
                .Any(c => c is FunctionResultContent);
            if (isLogAvailable)
            {
                return Task.FromResult(GetFinalAnswer(messages));
            }

            return Task.FromResult(GetLogGatheringRequest());
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return this;
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
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
                        new TextContent($"I have gathered the relevant logs here: ```{toolCallResponse}```")
                    ]
                }
                ]
            };
            return response;
        }
    }
}


