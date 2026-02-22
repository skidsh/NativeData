using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NativeData.Generators;

namespace NativeData.Tests;

public class GeneratorTests
{
    [Fact]
    public void Generator_EmitsEntityMap_ForAnnotatedRecordEntity()
    {
        const string source = """
using NativeData.Abstractions;

namespace Demo;

[NativeDataEntity("People", "Id")]
public sealed record Person(int Id, string Name);
""";

        var compilation = CreateCompilation(source);
        var generator = new NativeDataEntityGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out var updatedCompilation,
            out var generatorDiagnostics);

        Assert.Empty(generatorDiagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Empty(updatedCompilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error));

        var generatedSources = driver.GetRunResult().Results
            .SelectMany(result => result.GeneratedSources)
            .Select(generated => generated.SourceText.ToString())
            .ToArray();

        Assert.Contains(generatedSources, generated => generated.Contains("IEntityMap<global::Demo.Person>"));
        Assert.Contains(generatedSources, generated => generated.Contains("public static class NativeDataEntityMaps"));
        Assert.Contains(generatedSources, generated => generated.Contains("public static IEntityMap<T> Create<T>()"));
        Assert.Contains(generatedSources, generated => generated.Contains("public string TableName => \"People\";"));
        Assert.Contains(generatedSources, generated => generated.Contains("new(\"Id\", entity.Id)"));
        Assert.Contains(generatedSources, generated => generated.Contains("new(\"Name\", entity.Name)"));
    }

    [Fact]
    public void Generator_ReportsDiagnostic_ForUnsupportedEntityShape()
    {
        const string source = """
using NativeData.Abstractions;

namespace Demo;

[NativeDataEntity("People")]
public sealed class BrokenEntity
{
    public BrokenEntity(int age)
    {
    }

    public int Id { get; }
}
""";

        var compilation = CreateCompilation(source);
        var generator = new NativeDataEntityGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);

        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation,
            out _,
            out _);

        var diagnostics = driver.GetRunResult().Results.SelectMany(result => result.Diagnostics).ToArray();
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "NDG0001");
    }

    private static CSharpCompilation CreateCompilation(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(assembly => !assembly.IsDynamic && !string.IsNullOrWhiteSpace(assembly.Location))
            .Select(assembly => MetadataReference.CreateFromFile(assembly.Location))
            .Cast<MetadataReference>()
            .ToList();

        EnsureReference(references, typeof(object).Assembly);
        EnsureReference(references, typeof(Enumerable).Assembly);
        EnsureReference(references, typeof(NativeData.Abstractions.NativeDataEntityAttribute).Assembly);

        return CSharpCompilation.Create(
            assemblyName: "GeneratorTests.Dynamic",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
    }

    private static void EnsureReference(List<MetadataReference> references, Assembly assembly)
    {
        if (string.IsNullOrWhiteSpace(assembly.Location))
        {
            return;
        }

        var hasReference = references.Any(reference =>
            string.Equals((reference as PortableExecutableReference)?.FilePath, assembly.Location, StringComparison.OrdinalIgnoreCase));

        if (!hasReference)
        {
            references.Add(MetadataReference.CreateFromFile(assembly.Location));
        }
    }
}
