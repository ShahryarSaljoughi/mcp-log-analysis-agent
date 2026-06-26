using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pleng.Agent;

internal sealed partial class DataCollectionExecutor : Executor<ChatMessage, DataCollectionResult>
{
    private readonly AIAgent _dataCollectorAgent;

    public DataCollectionExecutor(AIAgent dataCollectorAgent) : base("DataCollectionExecutor")
    {
        this._dataCollectorAgent = dataCollectorAgent;
    }

    public override async ValueTask<DataCollectionResult> HandleAsync(ChatMessage message, IWorkflowContext context, CancellationToken cancellationToken = default)
    {
        var response = await _dataCollectorAgent.RunAsync(message);
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
