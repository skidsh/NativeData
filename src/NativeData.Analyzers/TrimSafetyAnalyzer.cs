using System.Collections.Immutable;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NativeData.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TrimSafetyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ND0001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        "Avoid runtime type loading",
        "Runtime type loading with Type.GetType is not AOT/trimming-safe",
        "NativeData.Compatibility",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use compile-time known types instead of Type.GetType for AOT/trimming compliance.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol symbol)
        {
            return;
        }

        if (symbol.Name != nameof(Type.GetType) || symbol.ContainingType.ToDisplayString() != "System.Type")
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Rule, invocation.GetLocation()));
    }
}