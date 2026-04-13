using Microsoft.SemanticKernel;
using CodeAegis.Plugins.Native;

namespace CodeAegis.Configuration;

public static class KernelSetup
{
    public static Kernel BuildLocalKernel()
    {
        var builder = Kernel.CreateBuilder();
        
        // Configure the local Ollama connection
        builder.AddOpenAIChatCompletion(
            modelId: "phi3", 
            apiKey: "NoKeyNeeded",
            endpoint: new Uri("http://localhost:11434/v1")
        );

        // Register all Native Plugins (Tools) here
        builder.Plugins.AddFromType<FileSystemPlugin>("FileSystem");

        return builder.Build();
    }
}