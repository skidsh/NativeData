# NativeData.Analyzers

Roslyn analyzer project for NativeData AOT/trimming safety diagnostics.

## Contents

- `TrimSafetyAnalyzer`
- Rule IDs in the `NDxxxx` namespace (starting with `ND0001`)

## Build

```bash
dotnet build src/NativeData.Analyzers/NativeData.Analyzers.csproj
```

## Packaging

This project is currently marked `IsPackable=false` in this repository’s release automation.
