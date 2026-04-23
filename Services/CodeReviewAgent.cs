using Microsoft.SemanticKernel;
using System.Text.Json;
using CodeAegis.Models;

namespace CodeAegis.Services;

public class CodeReviewAgent
{
    private readonly Kernel _kernel;
    private readonly SemanticChunker _chunker;

    public CodeReviewAgent(Kernel kernel)
    {
        _kernel = kernel;
        _chunker = new SemanticChunker();
    }

    public async Task RunAuditAsync(string targetFilePath)
    {
        Console.WriteLine($"\n[CodeAegis] Initializing scan on {targetFilePath}...");

        // 1. Read File
        var readArgs = new KernelArguments { { "filePath", targetFilePath } };
        var readResult = await _kernel.InvokeAsync("FileSystem", "read_file", readArgs);
        string codeContent = readResult.GetValue<string>() ?? string.Empty;

        if (codeContent.StartsWith("Error"))
        {
            Console.WriteLine(codeContent);
            return;
        }

        // 2. CHUNK THE CODE
        var codeChunks = _chunker.ChunkCodeByMethods(codeContent);
        Console.WriteLine($"[CodeAegis] File parsed successfully. Split into {codeChunks.Count} logical methods.\n");

        var semanticPluginsDir = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "Semantic");
        var semanticPlugin = _kernel.CreatePluginFromPromptDirectory(semanticPluginsDir, "SemanticPlugins");

        // 3. LOOP THROUGH CHUNKS (Updated!)
        foreach (var chunk in codeChunks)
        {
            // Now we print the actual method name!
            Console.WriteLine($"--- Auditing Method: {chunk.MethodName} ---");
            
            // Make sure to pass chunk.Code to the AI, not the whole object
            var scanArgs = new KernelArguments { { "codeSnippet", chunk.Code } };
            var scanResult = await _kernel.InvokeAsync(semanticPlugin["SecurityAuditor"], scanArgs);
            
            ProcessAndPrintResult(scanResult.GetValue<string>() ?? string.Empty);
        }
    }

    // Helper method to keep our loop clean
    private void ProcessAndPrintResult(string rawResponse)
    {
        string cleanJson = rawResponse;
        if (cleanJson.StartsWith("```json"))
        {
            cleanJson = cleanJson.Replace("```json", "").Replace("```", "").Trim();
        }

        try
        {
            var parsedReport = JsonSerializer.Deserialize<AuditResult>(cleanJson);
            
            if (parsedReport?.Vulnerabilities != null && parsedReport.Vulnerabilities.Count > 0)
            {
                Console.WriteLine($"[Found {parsedReport.Vulnerabilities.Count} Issue(s)]");
                foreach (var issue in parsedReport.Vulnerabilities)
                {
                    Console.WriteLine($"⚠️ {issue.Type}: {issue.Description}");
                }
            }
            else
            {
                Console.WriteLine("✅ No vulnerabilities found in this chunk.");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("[Error] The AI generated invalid JSON for this chunk.");
        }
        Console.WriteLine();
    }
}