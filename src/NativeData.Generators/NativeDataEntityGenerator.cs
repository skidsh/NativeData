using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace NativeData.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class NativeDataEntityGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor UnsupportedEntityShape = new(
        id: "NDG0001",
        title: "Unsupported NativeData entity shape",
        messageFormat: "Entity '{0}' must have either a public constructor with parameters that match public properties or a public parameterless constructor with settable public properties",
        category: "NativeData.Generators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor MissingKeyProperty = new(
        id: "NDG0002",
        title: "Missing key property",
        messageFormat: "Entity '{0}' is marked with key column '{1}', but no matching public property was found",
        category: "NativeData.Generators",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var entityCandidates = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) => node is TypeDeclarationSyntax typeDeclaration && typeDeclaration.AttributeLists.Count > 0,
            static (ctx, _) => GetEntityType(ctx))
            .Where(static symbol => symbol is not null)
            .Select(static (symbol, _) => symbol!);

        context.RegisterSourceOutput(entityCandidates.Collect(), static (productionContext, entities) => Emit(productionContext, entities));
    }

    private static INamedTypeSymbol? GetEntityType(GeneratorSyntaxContext context)
    {
        var typeDeclaration = (TypeDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(typeDeclaration) is not INamedTypeSymbol typeSymbol)
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

        var distinctEntities = entities.Distinct(SymbolEqualityComparer.Default).Cast<INamedTypeSymbol>().ToArray();
        var source = new StringBuilder();
        source.AppendLine("#nullable enable");
        source.AppendLine("using System;");
        source.AppendLine("using System.Collections.Generic;");
        source.AppendLine("using System.Data;");
        source.AppendLine("using NativeData.Abstractions;");
        source.AppendLine();
        source.AppendLine("namespace NativeData.Generated;");
        source.AppendLine();

        var generatedEntries = new List<(EntityShape Shape, string MapTypeName)>();

        foreach (var entity in distinctEntities)
        {
            var shape = BuildEntityShape(entity);
            if (shape is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    UnsupportedEntityShape,
                    entity.Locations.FirstOrDefault(),
                    entity.ToDisplayString()));
                continue;
            }

            if (!shape.Properties.Any(static p => p.IsKey))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    MissingKeyProperty,
                    entity.Locations.FirstOrDefault(),
                    entity.ToDisplayString(),
                    shape.KeyColumn));
                continue;
            }

            var mapTypeName = GetMapTypeName(entity);
            generatedEntries.Add((shape, mapTypeName));
            AppendEntityMap(source, shape, mapTypeName);
        }

        source.AppendLine("public static class NativeDataEntityRegistry");
        source.AppendLine("{");
        source.AppendLine("    public static string[] EntityTypeNames { get; } = new[]");
        source.AppendLine("    {");

        foreach (var entity in distinctEntities)
        {
            source.Append("        \"");
            source.Append(entity.ToDisplayString());
            source.AppendLine("\",");
        }

        source.AppendLine("    };");
        source.AppendLine();
        source.AppendLine("    public static Type[] GeneratedMapTypes { get; } = new[]");
        source.AppendLine("    {");
        foreach (var entry in generatedEntries)
        {
            source.Append("        typeof(global::NativeData.Generated.");
            source.Append(entry.MapTypeName);
            source.AppendLine("),");
        }

        source.AppendLine("    };\n}");
        source.AppendLine();
        source.AppendLine("public static class NativeDataEntityMaps");
        source.AppendLine("{");
        source.AppendLine("    public static IEntityMap<T> Create<T>()");
        source.AppendLine("        where T : class");
        source.AppendLine("    {");
        foreach (var entry in generatedEntries)
        {
            source.Append("        if (typeof(T) == typeof(");
            source.Append(entry.Shape.EntityTypeName);
            source.AppendLine("))");
            source.AppendLine("        {");
            source.Append("            return (IEntityMap<T>)(object)new ");
            source.Append(entry.MapTypeName);
            source.AppendLine("();");
            source.AppendLine("        }");
            source.AppendLine();
        }

        source.AppendLine("        throw new InvalidOperationException($\"No generated NativeData map exists for type '{typeof(T).FullName}'.\");");
        source.AppendLine("    }");
        source.AppendLine("}");

        context.AddSource("NativeDataEntityRegistry.g.cs", source.ToString());
    }

    private static EntityShape? BuildEntityShape(INamedTypeSymbol entity)
    {
        var entityAttribute = entity.GetAttributes().FirstOrDefault(static attribute =>
            attribute.AttributeClass?.ToDisplayString() is "NativeData.Abstractions.NativeDataEntityAttribute" or "NativeDataEntityAttribute");

        if (entityAttribute is null || entityAttribute.ConstructorArguments.Length == 0)
        {
            return null;
        }

        var tableName = entityAttribute.ConstructorArguments[0].Value as string;
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return null;
        }

        var keyColumn = entityAttribute.ConstructorArguments.Length > 1
            ? entityAttribute.ConstructorArguments[1].Value as string
            : "Id";

        keyColumn ??= "Id";

        var properties = entity.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(static property =>
                !property.IsStatic &&
                property.Parameters.Length == 0 &&
                property.GetMethod is not null &&
                property.GetMethod.DeclaredAccessibility == Accessibility.Public)
            .Select(property => new EntityProperty(
                property.Name,
                property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                property.SetMethod is not null && property.SetMethod.DeclaredAccessibility == Accessibility.Public,
                string.Equals(property.Name, keyColumn, StringComparison.OrdinalIgnoreCase)))
            .ToImmutableArray();

        if (properties.Length == 0)
        {
            return null;
        }

        var allPropertyNames = new HashSet<string>(
            properties.Select(static property => property.Name),
            StringComparer.OrdinalIgnoreCase);
        var constructors = entity.InstanceConstructors
            .Where(static ctor => ctor.DeclaredAccessibility == Accessibility.Public)
            .OrderByDescending(static ctor => ctor.Parameters.Length)
            .ToArray();

        var constructor = constructors.FirstOrDefault(ctor =>
            ctor.Parameters.All(parameter => allPropertyNames.Contains(parameter.Name)));

        var canUseParameterlessSetters = entity.InstanceConstructors.Any(static ctor =>
            ctor.DeclaredAccessibility == Accessibility.Public &&
            ctor.Parameters.Length == 0) && properties.Any(static property => property.IsSettable);

        if (constructor is null && !canUseParameterlessSetters)
        {
            return null;
        }

        var constructorParameterNames = constructor is null
            ? ImmutableArray<string>.Empty
            : constructor.Parameters.Select(static parameter => parameter.Name).ToImmutableArray();

        return new EntityShape(
            entity.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
            tableName!,
            keyColumn,
            properties,
            constructorParameterNames,
            constructor is not null,
            canUseParameterlessSetters);
    }

    private static void AppendEntityMap(StringBuilder source, EntityShape shape, string mapTypeName)
    {
        source.Append("file sealed class ");
        source.Append(mapTypeName);
        source.Append(" : IEntityMap<");
        source.Append(shape.EntityTypeName);
        source.AppendLine(">\n{");

        source.Append("    public string TableName => \"");
        source.Append(EscapeForCSharp(shape.TableName));
        source.AppendLine("\";");

        source.Append("    public string KeyColumn => \"");
        source.Append(EscapeForCSharp(shape.KeyColumn));
        source.AppendLine("\";");

        source.AppendLine();
        source.AppendLine("    public IReadOnlyList<string> WritableColumns => new[]");
        source.AppendLine("    {");
        foreach (var property in shape.Properties)
        {
            source.Append("        \"");
            source.Append(EscapeForCSharp(property.Name));
            source.AppendLine("\",");
        }

        source.AppendLine("    };");

        source.AppendLine();
        source.Append("    public object? GetKey(");
        source.Append(shape.EntityTypeName);
        source.AppendLine(" entity)");
        source.AppendLine("    {");
        var keyProperty = shape.Properties.First(property => property.IsKey);
        source.Append("        return entity.");
        source.Append(keyProperty.Name);
        source.AppendLine(";");
        source.AppendLine("    }");

        source.AppendLine();
        source.Append("    public IReadOnlyList<SqlParameterValue> BuildInsertParameters(");
        source.Append(shape.EntityTypeName);
        source.AppendLine(" entity)");
        source.AppendLine("    {");
        source.AppendLine("        return new SqlParameterValue[]");
        source.AppendLine("        {");
        foreach (var property in shape.Properties)
        {
            source.Append("            new(\"");
            source.Append(EscapeForCSharp(property.Name));
            source.Append("\", entity.");
            source.Append(property.Name);
            source.AppendLine("),");
        }

        source.AppendLine("        };\n    }");

        source.AppendLine();
        source.Append("    public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(");
        source.Append(shape.EntityTypeName);
        source.AppendLine(" entity)");
        source.AppendLine("    {");
        source.AppendLine("        return new SqlParameterValue[]");
        source.AppendLine("        {");
        foreach (var property in shape.Properties)
        {
            source.Append("            new(\"");
            source.Append(EscapeForCSharp(property.Name));
            source.Append("\", entity.");
            source.Append(property.Name);
            source.AppendLine("),");
        }

        source.AppendLine("        };\n    }");

        source.AppendLine();
        source.Append("    public ");
        source.Append(shape.EntityTypeName);
        source.AppendLine(" Materialize(IDataRecord record)");
        source.AppendLine("    {");

        if (shape.UsesConstructor)
        {
            source.Append("        return new ");
            source.Append(shape.EntityTypeName);
            source.AppendLine("(");

            for (var index = 0; index < shape.ConstructorParameterNames.Length; index++)
            {
                var parameterName = shape.ConstructorParameterNames[index];
                var property = shape.Properties.First(staticProperty =>
                    string.Equals(staticProperty.Name, parameterName, StringComparison.OrdinalIgnoreCase));

                source.Append("            ReadValue<");
                source.Append(property.TypeName);
                source.Append(">(record, \"");
                source.Append(EscapeForCSharp(property.Name));
                source.Append("\")");
                source.AppendLine(index == shape.ConstructorParameterNames.Length - 1 ? string.Empty : ",");
            }

            source.AppendLine("        );");
        }
        else
        {
            source.Append("        var entity = new ");
            source.Append(shape.EntityTypeName);
            source.AppendLine("();");

            foreach (var property in shape.Properties.Where(static property => property.IsSettable))
            {
                source.Append("        entity.");
                source.Append(property.Name);
                source.Append(" = ReadValue<");
                source.Append(property.TypeName);
                source.Append(">(record, \"");
                source.Append(EscapeForCSharp(property.Name));
                source.AppendLine("\");");
            }

            source.AppendLine();
            source.AppendLine("        return entity;");
        }

        source.AppendLine("    }");

        source.AppendLine();
        source.AppendLine("    private static T ReadValue<T>(IDataRecord record, string columnName)");
        source.AppendLine("    {");
        source.AppendLine("        var value = record[columnName];");
        source.AppendLine("        if (value is DBNull)");
        source.AppendLine("        {");
        source.AppendLine("            return default!;");
        source.AppendLine("        }");

        source.AppendLine();
        source.AppendLine("        if (value is T typedValue)");
        source.AppendLine("        {");
        source.AppendLine("            return typedValue;");
        source.AppendLine("        }");

        source.AppendLine();
        source.AppendLine("        var targetType = typeof(T);");
        source.AppendLine("        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;");

        source.AppendLine();
        source.AppendLine("        if (underlyingType.IsEnum)");
        source.AppendLine("        {");
        source.AppendLine("            var enumUnderlyingType = Enum.GetUnderlyingType(underlyingType);");
        source.AppendLine("            var enumValue = Convert.ChangeType(value, enumUnderlyingType);");
        source.AppendLine("            return (T)Enum.ToObject(underlyingType, enumValue!);");
        source.AppendLine("        }");

        source.AppendLine();
        source.AppendLine("        return (T)Convert.ChangeType(value, underlyingType);");
        source.AppendLine("    }");
        source.AppendLine("}");
        source.AppendLine();
    }

    private static string GetMapTypeName(INamedTypeSymbol entity)
    {
        var fullName = entity.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var builder = new StringBuilder(fullName.Length + 32);
        foreach (var character in fullName)
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '_');
        }

        builder.Append("_NativeDataMap");
        return builder.ToString();
    }

    private static string EscapeForCSharp(string value)
    {
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private sealed class EntityShape
    {
        public EntityShape(
            string entityTypeName,
            string tableName,
            string keyColumn,
            ImmutableArray<EntityProperty> properties,
            ImmutableArray<string> constructorParameterNames,
            bool usesConstructor,
            bool usesSetters)
        {
            EntityTypeName = entityTypeName;
            TableName = tableName;
            KeyColumn = keyColumn;
            Properties = properties;
            ConstructorParameterNames = constructorParameterNames;
            UsesConstructor = usesConstructor;
            UsesSetters = usesSetters;
        }

        public string EntityTypeName { get; }

        public string TableName { get; }

        public string KeyColumn { get; }

        public ImmutableArray<EntityProperty> Properties { get; }

        public ImmutableArray<string> ConstructorParameterNames { get; }

        public bool UsesConstructor { get; }

        public bool UsesSetters { get; }
    }

    private sealed class EntityProperty
    {
        public EntityProperty(string name, string typeName, bool isSettable, bool isKey)
        {
            Name = name;
            TypeName = typeName;
            IsSettable = isSettable;
            IsKey = isKey;
        }

        public string Name { get; }

        public string TypeName { get; }

        public bool IsSettable { get; }

        public bool IsKey { get; }
    }
}