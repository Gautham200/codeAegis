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

    public async Task RunDirectoryAuditAsync(string targetDirectoryPath)
    {
        Console.WriteLine($"\n[CodeAegis] Initializing Repository Scan on: {targetDirectoryPath}\n");

        // 1. Get all C# files using our new Crawler tool
        var dirArgs = new KernelArguments { { "directoryPath", targetDirectoryPath } };
        var filesResult = await _kernel.InvokeAsync("FileSystem", "get_csharp_files", dirArgs);
        
        string[] files = filesResult.GetValue<string[]>() ?? Array.Empty<string>();

        if (files.Length == 0)
        {
            Console.WriteLine("[CodeAegis] No C# files found to audit. Exiting.");
            return;
        }

        // Pre-load the AI Brain so we don't do it inside the loop
        var semanticPluginsDir = Path.Combine(Directory.GetCurrentDirectory(), "Plugins", "Semantic");
        var semanticPlugin = _kernel.CreatePluginFromPromptDirectory(semanticPluginsDir, "SemanticPlugins");

        // 2. Loop through every file found in the directory
        foreach (var file in files)
        {
            Console.WriteLine($"\n==================================================");
            Console.WriteLine($"🔍 AUDITING FILE: {Path.GetFileName(file)}");
            Console.WriteLine($"==================================================\n");

            // A. Read the current file
            var readArgs = new KernelArguments { { "filePath", file } };
            var readResult = await _kernel.InvokeAsync("FileSystem", "read_file", readArgs);
            string codeContent = readResult.GetValue<string>() ?? string.Empty;

            if (codeContent.StartsWith("Error") || string.IsNullOrWhiteSpace(codeContent))
            {
                Console.WriteLine($"[Warning] Skipping file due to read error.");
                continue;
            }

            // B. Chunk the file
            var codeChunks = _chunker.ChunkCodeByMethods(codeContent);
            Console.WriteLine($"[CodeAegis] Parsed into {codeChunks.Count} logical methods.\n");

            // C. Audit each chunk
            foreach (var chunk in codeChunks)
            {
                Console.WriteLine($"--- Auditing Method: {chunk.MethodName} ---");
                
                var scanArgs = new KernelArguments { { "codeSnippet", chunk.Code } };
                var scanResult = await _kernel.InvokeAsync(semanticPlugin["SecurityAuditor"], scanArgs);
                
                ProcessAndPrintResult(scanResult.GetValue<string>() ?? string.Empty);
            }
        }
        
        Console.WriteLine("\n[CodeAegis] Repository Audit Complete.");
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