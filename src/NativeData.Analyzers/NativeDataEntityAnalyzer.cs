using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NativeData.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NativeDataEntityAnalyzer : DiagnosticAnalyzer
{
    public const string NativeDataEntityKeyPropertyDiagnosticId = "ND1001";
    public const string NativeDataEntityInvalidLiteralDiagnosticId = "ND1002";

    private static readonly DiagnosticDescriptor NativeDataEntityKeyPropertyRule = new(
        NativeDataEntityKeyPropertyDiagnosticId,
        "NativeData entity key column must map to a public property",
        "Entity '{0}' is marked with [NativeDataEntity] key column '{1}', but no matching public readable property was found",
        "NativeData.Mapping",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ensure the [NativeDataEntity] key column matches a public readable property so generated mapping can reliably resolve entity keys.");

    private static readonly DiagnosticDescriptor NativeDataEntityInvalidLiteralRule = new(
        NativeDataEntityInvalidLiteralDiagnosticId,
        "NativeData entity attribute values must be non-empty",
        "Entity '{0}' has invalid [NativeDataEntity] value for '{1}'; use a non-empty, non-whitespace literal",
        "NativeData.Mapping",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ensure [NativeDataEntity] tableName and keyColumn literals are non-empty and non-whitespace.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [NativeDataEntityKeyPropertyRule, NativeDataEntityInvalidLiteralRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol)
        {
            return;
        }

        var entityAttribute = typeSymbol.GetAttributes().FirstOrDefault(static attribute =>
            attribute.AttributeClass?.ToDisplayString() is "NativeData.Abstractions.NativeDataEntityAttribute" or "NativeDataEntityAttribute");

        if (entityAttribute is null)
        {
            return;
        }

        var tableName = entityAttribute.ConstructorArguments.Length > 0
            ? entityAttribute.ConstructorArguments[0].Value as string
            : null;

        if (string.IsNullOrWhiteSpace(tableName))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                NativeDataEntityInvalidLiteralRule,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name,
                "tableName"));
        }

        var keyColumn = entityAttribute.ConstructorArguments.Length > 1
            ? entityAttribute.ConstructorArguments[1].Value as string
            : "Id";

        if (entityAttribute.ConstructorArguments.Length > 1 && string.IsNullOrWhiteSpace(keyColumn))
        {
            context.ReportDiagnostic(Diagnostic.Create(
                NativeDataEntityInvalidLiteralRule,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name,
                "keyColumn"));
        }

        keyColumn = string.IsNullOrWhiteSpace(keyColumn) ? "Id" : keyColumn;

        var hasMatchingProperty = typeSymbol.GetMembers()
            .OfType<IPropertySymbol>()
            .Any(property =>
                !property.IsStatic &&
                property.GetMethod is not null &&
                property.GetMethod.DeclaredAccessibility == Accessibility.Public &&
                string.Equals(property.Name, keyColumn, StringComparison.OrdinalIgnoreCase));

        if (!hasMatchingProperty)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                NativeDataEntityKeyPropertyRule,
                typeSymbol.Locations.FirstOrDefault(),
                typeSymbol.Name,
                keyColumn));
        }
    }
}
