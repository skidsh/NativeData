using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NativeData.Generators;

[Generator]
public sealed class NativeDataEntityGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityCandidates = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is ClassDeclarationSyntax classDeclaration && classDeclaration.AttributeLists.Count > 0,
            static (ctx, _) => GetEntityType(ctx))
            .Where(static symbol => symbol is not null)
            .Select(static (symbol, _) => symbol!);

        context.RegisterSourceOutput(entityCandidates.Collect(), static (productionContext, entities) => Emit(productionContext, entities));
    }

    private static INamedTypeSymbol? GetEntityType(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        foreach (var attribute in typeSymbol.GetAttributes())
        {
            var name = attribute.AttributeClass?.ToDisplayString();
            if (name is "NativeData.Abstractions.NativeDataEntityAttribute" or "NativeDataEntityAttribute")
            {
                return typeSymbol;
            }
        }

        return null;
    }

    private static void Emit(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> entities)
    {
        if (entities.IsDefaultOrEmpty)
        {
            return;
        }

        var source = new StringBuilder();
        source.AppendLine("namespace NativeData.Generated;");
        source.AppendLine();
        source.AppendLine("public static class NativeDataEntityRegistry");
        source.AppendLine("{");
        source.AppendLine("    public static string[] EntityTypeNames { get; } = new[]");
        source.AppendLine("    {");

        foreach (var entity in entities.Distinct(SymbolEqualityComparer.Default))
        {
            source.Append("        \"");
            source.Append(entity?.ToDisplayString() ?? string.Empty);
            source.AppendLine("\",");
        }

        source.AppendLine("    };");
        source.AppendLine("}");

        context.AddSource("NativeDataEntityRegistry.g.cs", source.ToString());
    }
}