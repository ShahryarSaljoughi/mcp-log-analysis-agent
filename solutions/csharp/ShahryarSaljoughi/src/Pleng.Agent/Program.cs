using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;
using Pleng.Agent;
using System.ClientModel;
using System.Text.Json;

var agentConfig = new Config(
    openAIApiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
    openAIEndpointBase: Environment.GetEnvironmentVariable("OPENAI_ENDPOINT"),
    backendType: Enum.Parse<BackendType>(Environment.GetEnvironmentVariable("BackendType") ?? "Fake") ,
    openAIModel: "gpt-5-mini"
    );

var logInvestigationAgent = await new LogInvestigatorAgentCreator().CreateAsync(agentConfig);

//var prompt1 = "Why has the payment service been returning HTTP 500 errors during the last 15 minutes?";
//var prompt2 = "What is wrong with my services?";
//var prompt1 = args[0];
//var agentResponse = await logInvestigationAgent.RunAsync(prompt1);

//Console.WriteLine(agentResponse);

var dataCollectorAgent = await new DataCollectorCreator().CreateAsync(agentConfig);


var dataCollectionExecutor = new DataCollectionExecutor(dataCollectorAgent);    
var logInvestigationExecutor = new LogInvestigationExecutor(logInvestigationAgent);
var workflow = new WorkflowBuilder(dataCollectionExecutor)
    .AddEdge(dataCollectionExecutor, logInvestigationExecutor)
    .WithOutputFrom(logInvestigationExecutor)
    .Build();
string initialMessage = "Why has the payment service been returning HTTP 500 errors during the last 15 minutes?";
initialMessage = "Why has the payment service been returning HTTP 500 errors?";
StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new ChatMessage(ChatRole.User, initialMessage));
await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

await foreach (WorkflowEvent evt in run.WatchStreamAsync().ConfigureAwait(false))
{
    if (evt is WorkflowOutputEvent outputEvent)
    {
        Console.WriteLine($"{outputEvent}");
    }
}

