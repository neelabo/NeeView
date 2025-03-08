using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NeeLaboratory.SourceGenerator.Tools;
using System;
using System.Diagnostics;
using System.Linq;

namespace NeeLaboratory.SourceGenerator;

[Generator]
public sealed class LocalDebugGenerator : IIncrementalGenerator
{
    private static readonly string _generatorNamespace = "NeeLaboratory.Generators";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource("LocalDebugAttribute.g.cs", $$"""
            using System;
            namespace {{_generatorNamespace}};
            
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            internal sealed class LocalDebugAttribute : Attribute
            {
            }
            """);
        });

        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{_generatorNamespace}.LocalDebugAttribute",
            static (node, token) => true,
            static (context, token) => context);

        context.RegisterSourceOutput(source, Emit);
    }

    static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source)
    {
        var typeSymbol = (INamedTypeSymbol)source.TargetSymbol;
        //var typeNode = (TypeDeclarationSyntax)source.TargetNode;

        var ns = typeSymbol.ContainingNamespace.IsGlobalNamespace
            ? ""
            : $"namespace {typeSymbol.ContainingNamespace};";

        var name = typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        var global = "global::";
        var fullType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (fullType.StartsWith(global))
        {
            fullType = fullType.Substring(global.Length);
        }

        string typeString = "";
        if (typeSymbol.IsStatic)
        {
            typeString += "static ";
        }
        typeString += "partial ";
        if (typeSymbol.IsRecord)
        {
            typeString += "record ";
        }
        typeString += typeSymbol.IsValueType ? "struct" : "class";

        var code = $$"""
            #nullable enable
            using System;
            using System.Diagnostics;

            {{ns}}

            {{typeString}} {{name}}
            {
                private static class LocalDebug
                {
                    [Conditional("LOCAL_DEBUG")]
                    internal static void WriteLine(string s, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "")
                    {
                        Debug.WriteLine($"{{name}}.{memberName}: {s}");
                    }
                }
            }
            """;

        context.AddSource(PathTools.ReplaceInvalidFileNameChars($"{fullType}.LocalDebug.g.cs"), code);
    }

}