using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAegis.Services;

public class SemanticChunker
{
    public record CodeChunk(string MethodName, string Code);
    public List<CodeChunk> ChunkCodeByMethods(string rawCode)
    {
        var chunks = new List<CodeChunk>();
        
        // 1. Parse the raw string into an Abstract Syntax Tree (AST)
        SyntaxTree tree = CSharpSyntaxTree.ParseText(rawCode);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        // 2. Find all Method Declarations in the file
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        // 3. Extract the clean text for each method
        foreach (var method in methods)
        {
            string methodName = method.Identifier.Text;
            string methodCode = method.ToFullString().Trim();
            // We include the method signature and its full body
            chunks.Add(new CodeChunk(methodName, methodCode));
        }

        return chunks;
    }
}