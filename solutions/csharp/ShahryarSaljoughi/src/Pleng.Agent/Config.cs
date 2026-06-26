using System;
using System.Collections.Generic;
using System.Text;

namespace Pleng.Agent;

public class Config
{
    public BackendType BackendType { get; }
    public string? OpenAIEndpointBase { get; }
    public string? OpenAIApiKey { get; }
    public string? OpenAIModel { get; }
    public Config(string? openAIApiKey, string? openAIEndpointBase, BackendType backendType, string? openAIModel)
    {
        BackendType = backendType;
        if (backendType == BackendType.Fake) return;
        if (string.IsNullOrWhiteSpace(openAIApiKey))
            throw new ArgumentException(null, nameof(openAIApiKey));
        if (string.IsNullOrWhiteSpace(openAIEndpointBase))
            throw new ArgumentException(null, nameof(openAIEndpointBase));
        OpenAIApiKey = openAIApiKey;
        OpenAIEndpointBase = openAIEndpointBase;
        OpenAIModel = string.IsNullOrEmpty(openAIModel) ? "gpt-5-mini" : openAIModel;
    }
}

public enum BackendType
{
    OpenAI = 1,
    Fake = 2
}
