using Microsoft.SemanticKernel;

namespace CodeAegis.Services;

public class CodeReviewAgent
{
    private readonly Kernel _kernel;

    // The Kernel is injected, making this class highly testable
    public CodeReviewAgent(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task RunAuditAsync(string targetFilePath)
    {
        Console.WriteLine($"\n[CodeAegis] Initializing scan on {targetFilePath}...");

        // 1. Read the file using the Native Plugin
        var readArgs = new KernelArguments { { "filePath", targetFilePath } };
        var readResult = await _kernel.InvokeAsync("FileSystem", "read_file", readArgs);
        
        string codeContent = readResult.GetValue<string>() ?? string.Empty;

        if (codeContent.StartsWith("Error"))
        {
            Console.WriteLine(codeContent);
            return;
        }

        Console.WriteLine("[CodeAegis] File read successfully. Analyzing code...\n");

        // 2. Load the Semantic Plugin from the PARENT directory
        var semanticPluginsDir = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "Semantic");
        
        // This creates a plugin and registers EVERY sub-folder as a distinct function
        var semanticPlugin = _kernel.CreatePluginFromPromptDirectory(semanticPluginsDir, "SemanticPlugins");

        // 3. Execute the Prompt
        var scanArgs = new KernelArguments { { "codeSnippet", codeContent } };
        
        // Call the specific function (which perfectly matches your folder name)
        var scanResult = await _kernel.InvokeAsync(semanticPlugin["SecurityAuditor"], scanArgs);

        Console.WriteLine("================ AUDIT REPORT ================\n");
        Console.WriteLine("================ AUDIT REPORT ================\n");
        Console.WriteLine(scanResult);
        Console.WriteLine("\n==============================================");
    }
}