using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pleng.Agent;

//internal sealed partial class DataCollectionFaultTerminator : Executor<DataCollectionResult, FinalAnswer>
//{
//    public override ValueTask<FinalAnswer> HandleAsync(DataCollectionResult message, IWorkflowContext context, CancellationToken cancellationToken = default)
//    {
        
//    }
//}

/// <summary>
/// Executor that assists with ensuring needed data are provided by user.
/// </summary>
internal sealed partial class DataCollectionExecutor : Executor<ChatMessage, DataCollectionResult>
{
    private readonly AIAgent _dataCollectorAgent;

    public DataCollectionExecutor(AIAgent spamDetectionAgent) : base("DataCollectionExecutor")
    {
        this._dataCollectorAgent = spamDetectionAgent;
    }

    public override async ValueTask<DataCollectionResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        // Invoke the agent for spam detection
        var response = await this._dataCollectorAgent.RunAsync(message);
        var dataCollectionResult = JsonSerializer.Deserialize<DataCollectionResult>(
            response.Text, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        dataCollectionResult!.InitialUserMessage = message.Text;
        return dataCollectionResult;
    }
}

internal class DataCollectionResult
{
    public string? ServiceName { get; set; }
    public int? MinutesAgo { get; set; }
    public List<string> MissingInputs { get; set; } = [];
    [JsonIgnore]
    public string InitialUserMessage { get; set; } = string.Empty;
}
