using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodeAegis.Services;

public class SemanticChunker
{
    public List<string> ChunkCodeByMethods(string rawCode)
    {
        var chunks = new List<string>();
        
        // 1. Parse the raw string into an Abstract Syntax Tree (AST)
        SyntaxTree tree = CSharpSyntaxTree.ParseText(rawCode);
        CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

        // 2. Find all Method Declarations in the file
        var methods = root.DescendantNodes().OfType<MethodDeclarationSyntax>();

        // 3. Extract the clean text for each method
        foreach (var method in methods)
        {
            // We include the method signature and its full body
            chunks.Add(method.ToFullString().Trim());
        }

        return chunks;
    }
}