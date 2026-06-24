using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Responses;
using Pleng.Agent;
using System.ClientModel;

var agentConfig = new Config(
    openAIApiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    openAIEndpointBase: Environment.GetEnvironmentVariable("OPENAI_ENDPOINT"),
    backendType: BackendType.Fake,
    openAIModel: "gpt-5-mini"
    );

var platformEngineer = await new PlatformEngineerCreator().CreateAsync(agentConfig);

var prompt1 = "Why has the payment service been returning HTTP 500 errors during the last 15 minutes?";
var prompt2 = "What is wrong with my services?";
var agentResponse = await platformEngineer.RunAsync(prompt1);
Console.WriteLine(agentResponse);

