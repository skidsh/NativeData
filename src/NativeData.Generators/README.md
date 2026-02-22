# NativeData.Generators

Roslyn source generator project for NativeData entity mapping generation.

## Contents

- `NativeDataEntityGenerator`
- Discovery and generation pipeline for `[NativeDataEntity]`-annotated types

## Build

```bash
dotnet build src/NativeData.Generators/NativeData.Generators.csproj
```

## Packaging

This project is packable and emits source-generator artifacts under `analyzers/dotnet/cs` in the NuGet package.
