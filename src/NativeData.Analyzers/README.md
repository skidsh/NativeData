# NativeData.Analyzers

Roslyn analyzer project for NativeData AOT/trimming safety diagnostics.

## Contents

- `TrimSafetyAnalyzer` (`ND000x` compatibility rules)
- `NativeDataEntityAnalyzer` (`ND100x` NativeData mapping rules)
- Rule IDs in the `NDxxxx` namespace (starting with `ND0001`)

## Rules

- `ND0001` — Avoid runtime type loading (`Type.GetType(...)`)
	- Remediation: [docs/analyzers/ND0001.md](../../docs/analyzers/ND0001.md)
- `ND0002` — Avoid runtime assembly loading (`Assembly.Load(string)`)
	- Remediation: [docs/analyzers/ND0002.md](../../docs/analyzers/ND0002.md)
- `ND0003` — Avoid string-based runtime activation (`Activator.CreateInstance(string, string)`)
	- Remediation: [docs/analyzers/ND0003.md](../../docs/analyzers/ND0003.md)
- `ND1001` — NativeData entity key column must map to a public property
	- Remediation: [docs/analyzers/ND1001.md](../../docs/analyzers/ND1001.md)
- `ND1002` — NativeDataEntity `tableName`/`keyColumn` literals must be non-empty
	- Remediation: [docs/analyzers/ND1002.md](../../docs/analyzers/ND1002.md)

## Build

```bash
dotnet build src/NativeData.Analyzers/NativeData.Analyzers.csproj
```

## Packaging

This project is packable and emits analyzer artifacts under `analyzers/dotnet/cs` in the NuGet package.
