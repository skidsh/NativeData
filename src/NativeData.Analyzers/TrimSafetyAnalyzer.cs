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
    public const string TypeGetTypeDiagnosticId = "ND0001";
    public const string AssemblyLoadDiagnosticId = "ND0002";
    public const string ActivatorCreateInstanceDiagnosticId = "ND0003";
    public const string DiagnosticId = TypeGetTypeDiagnosticId;

    private static readonly DiagnosticDescriptor TypeGetTypeRule = new(
        TypeGetTypeDiagnosticId,
        "Avoid runtime type loading",
        "Runtime type loading with Type.GetType is not AOT/trimming-safe",
        "NativeData.Compatibility",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use compile-time known types instead of Type.GetType for AOT/trimming compliance.");

    private static readonly DiagnosticDescriptor AssemblyLoadRule = new(
        AssemblyLoadDiagnosticId,
        "Avoid runtime assembly loading",
        "Runtime assembly loading with Assembly.Load(string) is not AOT/trimming-safe",
        "NativeData.Compatibility",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use compile-time known assembly references instead of Assembly.Load(string) for AOT/trimming compliance.");

    private static readonly DiagnosticDescriptor ActivatorCreateInstanceRule = new(
        ActivatorCreateInstanceDiagnosticId,
        "Avoid string-based runtime activation",
        "String-based activation with Activator.CreateInstance is not AOT/trimming-safe",
        "NativeData.Compatibility",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Use compile-time known types instead of string-based Activator.CreateInstance overloads for AOT/trimming compliance.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [TypeGetTypeRule, AssemblyLoadRule, ActivatorCreateInstanceRule];

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

        if (symbol.Name == nameof(Type.GetType) && symbol.ContainingType.ToDisplayString() == "System.Type")
        {
            context.ReportDiagnostic(Diagnostic.Create(TypeGetTypeRule, invocation.GetLocation()));
            return;
        }

        if (symbol.Name == "Load" &&
            symbol.ContainingType.ToDisplayString() == "System.Reflection.Assembly" &&
            symbol.Parameters.Length == 1 &&
            symbol.Parameters[0].Type.SpecialType == SpecialType.System_String)
        {
            context.ReportDiagnostic(Diagnostic.Create(AssemblyLoadRule, invocation.GetLocation()));
            return;
        }

        if (symbol.Name == nameof(Activator.CreateInstance) &&
            symbol.ContainingType.ToDisplayString() == "System.Activator" &&
            symbol.Parameters.Length > 0 &&
            symbol.Parameters[0].Type.SpecialType == SpecialType.System_String)
        {
            context.ReportDiagnostic(Diagnostic.Create(ActivatorCreateInstanceRule, invocation.GetLocation()));
        }
    }
}