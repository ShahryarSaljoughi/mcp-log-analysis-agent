using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pleng.Agent;

/// <summary>
/// Executor that Investigating Logs
/// </summary>
public sealed partial class LogInvestigationExecutor : Executor<DataCollectionResult, FinalAnswer>
{
    private readonly AIAgent _logInvestigationAgent;

    public LogInvestigationExecutor(AIAgent emailAssistantAgent) : base("LogInvestigationExecutor")
    {
        this._logInvestigationAgent = emailAssistantAgent;
    }

    [MessageHandler]
    public override async ValueTask<FinalAnswer> HandleAsync(DataCollectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (message.MissingInputs?.Count > 0)
        {
            return new FinalAnswer { ErrorMessage = "You've not provided the required data" };
        }
        var prompt = @$"Users initial message is :
`
 ""InitialUserMessage"": ""{message.InitialUserMessage }""

`
and the relevant data based on user's message are:
`
 ""ServiceName"": ""{message.ServiceName }"",
  ""MinutesAgo"": {message.MinutesAgo}""
";
        var rawResponse = await _logInvestigationAgent.RunAsync(prompt);
        var finalAnswer = JsonSerializer.Deserialize<FinalAnswer>(
            rawResponse.Text, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        return finalAnswer;
    }
}

public class FinalAnswer
{
    public string FinalAnswerText { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    [JsonIgnore]
    public string? Text => !string.IsNullOrEmpty(ErrorMessage) ? $"[ERROR] {ErrorMessage}" : $"[ANSWER] {FinalAnswerText}";
}

