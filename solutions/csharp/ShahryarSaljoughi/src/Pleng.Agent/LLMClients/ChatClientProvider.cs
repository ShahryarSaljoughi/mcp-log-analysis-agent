using Microsoft.Extensions.AI;

namespace Pleng.Agent.LLMClients;

internal abstract class ChatClientProvider
{
    public abstract IChatClient Create(Config config);
}
