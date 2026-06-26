using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;
using Pleng.Agent;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace Pleng.Tests
{
    public class AgentTests
    {
        private static void ResetFakeLLMStep(int value)
        {
            var typeName = "Pleng.Agent.LLMClients.FakeLLMClientProvider+FakeClientHelper, Pleng.Agent";
            var helperType = Type.GetType(typeName);
            if (helperType is null)
                return;
            var field = helperType.GetField("_currentStep", BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, value);
        }

        [Fact]
        public void Config_OpenAI_Requires_Keys()
        {
            Assert.Throws<ArgumentException>(() => new Config(null, null, BackendType.OpenAI, null));
        }

        [Fact]
        public void Config_Fake_Does_Not_Throw()
        {
            var cfg = new Config(null, null, BackendType.Fake, null);
            Assert.Equal(BackendType.Fake, cfg.BackendType);
        }

        [Fact]
        public async Task DataCollectionExecutor_Returns_ParsedResult()
        {
            // ensure fake client starts at the correct step
            ResetFakeLLMStep(0);

            var creator = new DataCollectorAgentCreator();
            var agent = await creator.CreateAsync(new Config(null, null, BackendType.Fake, null));
            var executor = new DataCollectionExecutor(agent);

            var chatMsg = Activator.CreateInstance(typeof(ChatMessage)!, ChatRole.User, "Why has the payment service been returning HTTP 500 errors during the last 15 minutes?");
            dynamic dynExecutor = executor;
            var result = await dynExecutor.HandleAsync((dynamic)chatMsg, (IWorkflowContext?)null);

            Assert.NotNull(result);
            Assert.Equal("Payment", result.ServiceName, ignoreCase: true);
            Assert.Equal(15, result.MinutesAgo);
        }

        [Fact]
        public async Task LogInvestigationExecutor_MissingInputs_Returns_Error()
        {
            var creator = new DataCollectorAgentCreator();
            var agent = await creator.CreateAsync(new Config(null, null, BackendType.Fake, null));
            var executor = new LogInvestigationExecutor(agent);

            var data = new DataCollectionResult
            {
                ServiceName = null,
                MinutesAgo = null,
            };
            data.MissingInputs.Add("MinutesAgo");

            var result = await executor.HandleAsync(data, (IWorkflowContext?)null);

            Assert.NotNull(result);
            Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.Contains("not provided", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task LogInvestigationExecutor_WithAgent_Returns_FinalAnswer()
        {
            // set fake client to final-answer step
            ResetFakeLLMStep(2);

            var creator = new DataCollectorAgentCreator();
            var agent = await creator.CreateAsync(new Config(null, null, BackendType.Fake, null));
            var executor = new LogInvestigationExecutor(agent);

            var data = new DataCollectionResult
            {
                ServiceName = "Payment",
                MinutesAgo = 15,
                InitialUserMessage = "Why payment service returning 500?"
            };

            var result = await executor.HandleAsync(data, (IWorkflowContext?)null);

            Assert.NotNull(result);
            Assert.True(string.IsNullOrEmpty(result.ErrorMessage));
            Assert.False(string.IsNullOrEmpty(result.FinalAnswerText));
            Assert.Contains("gathered", result.FinalAnswerText, StringComparison.OrdinalIgnoreCase);
        }
    }
}
