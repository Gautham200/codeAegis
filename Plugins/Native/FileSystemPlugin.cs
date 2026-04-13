using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace CodeAegis.Plugins.Native;

public class FileSystemPlugin
{
    // The [KernelFunction] attribute registers this as a tool the AI can use
    [KernelFunction("read_file")]
    
    // The [Description] attribute is what the AI reads to understand what this tool does
    [Description("Reads the text content of a local file from the file system.")]
    public async Task<string> ReadFileAsync(
        [Description("The absolute or relative path to the file to read")] string filePath)
    {
        try
        {
            Console.WriteLine($"\n[FileSystem Tool] -> Attempting to read: {filePath}");

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return "Error: The file path provided was empty.";
            }

            // Verify the file actually exists before trying to read it
            if (!File.Exists(filePath))
            {
                return $"Error: File not found at '{filePath}'. Ensure the path is correct.";
            }

            // Use asynchronous reading for better performance and resource management
            string content = await File.ReadAllTextAsync(filePath);
            
            Console.WriteLine($"[FileSystem Tool] -> Successfully loaded {content.Length} characters into memory.");
            return content;
        }
        catch (UnauthorizedAccessException)
        {
            return $"Error: Access denied to file '{filePath}'. Check your read permissions.";
        }
        catch (Exception ex)
        {
            return $"Error reading file: {ex.Message}";
        }
    }
}