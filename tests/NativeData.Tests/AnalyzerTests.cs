using System.Reflection;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NativeData.Analyzers;

namespace NativeData.Tests;

public class AnalyzerTests
{
    [Fact]
    public async Task ND0001_ReportsDiagnostic_ForTypeGetTypeUsage()
    {
        const string source = """
using System;

public static class Demo
{
    public static Type? Resolve(string name)
    {
        return Type.GetType(name);
    }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == TrimSafetyAnalyzer.DiagnosticId);
    }

    [Fact]
    public async Task ND0001_DoesNotReportDiagnostic_WhenTypeGetTypeNotUsed()
    {
        const string source = """
using System;

public static class Demo
{
    public static Type Resolve()
    {
        return typeof(string);
    }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == TrimSafetyAnalyzer.DiagnosticId);
    }

    [Fact]
    public async Task ND0002_ReportsDiagnostic_ForAssemblyLoadStringUsage()
    {
        const string source = """
using System.Reflection;

public static class Demo
{
    public static Assembly Resolve(string name)
    {
        return Assembly.Load(name);
    }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == TrimSafetyAnalyzer.AssemblyLoadDiagnosticId);
    }

    [Fact]
    public async Task ND0002_DoesNotReportDiagnostic_ForStaticAssemblyReference()
    {
        const string source = """
using System.Reflection;

public static class Demo
{
    public static Assembly Resolve()
    {
        return typeof(string).Assembly;
    }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == TrimSafetyAnalyzer.AssemblyLoadDiagnosticId);
    }

    [Fact]
    public async Task ND0003_ReportsDiagnostic_ForStringBasedActivatorCreateInstance()
    {
        const string source = """
using System;

public static class Demo
{
    public static object? Create(string assemblyName, string typeName)
    {
        return Activator.CreateInstance(assemblyName, typeName);
    }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == TrimSafetyAnalyzer.ActivatorCreateInstanceDiagnosticId);
    }

    [Fact]
    public async Task ND0003_DoesNotReportDiagnostic_ForTypeBasedActivatorCreateInstance()
    {
        const string source = """
using System;

public static class Demo
{
    public static object? Create()
    {
        return Activator.CreateInstance(typeof(string));
    }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == TrimSafetyAnalyzer.ActivatorCreateInstanceDiagnosticId);
    }

    [Fact]
    public async Task ND1001_ReportsDiagnostic_WhenNativeDataEntityKeyPropertyMissing()
    {
        const string source = """
using NativeData.Abstractions;

[NativeDataEntity("Widgets", "WidgetId")]
public sealed class Widget
{
    public int Id { get; set; }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == NativeDataEntityAnalyzer.NativeDataEntityKeyPropertyDiagnosticId);
    }

    [Fact]
    public async Task ND1001_DoesNotReportDiagnostic_WhenNativeDataEntityKeyPropertyExists()
    {
        const string source = """
using NativeData.Abstractions;

[NativeDataEntity("Widgets", "WidgetId")]
public sealed class Widget
{
    public int WidgetId { get; set; }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == NativeDataEntityAnalyzer.NativeDataEntityKeyPropertyDiagnosticId);
    }

    [Fact]
    public async Task ND1002_ReportsDiagnostic_WhenNativeDataEntityTableNameIsWhitespace()
    {
        const string source = """
using NativeData.Abstractions;

[NativeDataEntity("   ", "Id")]
public sealed class Widget
{
    public int Id { get; set; }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == NativeDataEntityAnalyzer.NativeDataEntityInvalidLiteralDiagnosticId);
    }

    [Fact]
    public async Task ND1002_ReportsDiagnostic_WhenNativeDataEntityKeyColumnIsWhitespace()
    {
        const string source = """
using NativeData.Abstractions;

[NativeDataEntity("Widgets", "   ")]
public sealed class Widget
{
    public int Id { get; set; }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == NativeDataEntityAnalyzer.NativeDataEntityInvalidLiteralDiagnosticId);
    }

    [Fact]
    public async Task ND1002_DoesNotReportDiagnostic_WhenNativeDataEntityLiteralsAreValid()
    {
        const string source = """
using NativeData.Abstractions;

[NativeDataEntity("Widgets", "Id")]
public sealed class Widget
{
    public int Id { get; set; }
}
""";

        var diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Id == NativeDataEntityAnalyzer.NativeDataEntityInvalidLiteralDiagnosticId);
    }

    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests.Dynamic",
            syntaxTrees: [syntaxTree],
            references: GetMetadataReferences(),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzers = ImmutableArray.Create<DiagnosticAnalyzer>(
            new TrimSafetyAnalyzer(),
            new NativeDataEntityAnalyzer());
        var compilationWithAnalyzers = compilation.WithAnalyzers(analyzers);

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        var references = new List<MetadataReference>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic || string.IsNullOrWhiteSpace(assembly.Location))
            {
                continue;
            }

            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }

        EnsureReference(references, typeof(object).Assembly);
        EnsureReference(references, typeof(Type).Assembly);
        EnsureReference(references, typeof(Enumerable).Assembly);
        EnsureReference(references, typeof(NativeData.Abstractions.NativeDataEntityAttribute).Assembly);

        return references;
    }

    private static void EnsureReference(List<MetadataReference> references, Assembly assembly)
    {
        if (string.IsNullOrWhiteSpace(assembly.Location))
        {
            return;
        }

        var exists = references.Any(reference =>
            string.Equals((reference as PortableExecutableReference)?.FilePath, assembly.Location, StringComparison.OrdinalIgnoreCase));

        if (!exists)
        {
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }
    }
}
