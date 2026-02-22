# NativeData.AnalyzerSmoke

Minimal consumer smoke project that references `NativeData.Analyzers` as an analyzer.

## Purpose

- Verify analyzer consumption in a real consumer project shape
- Confirm no false-positive diagnostics for valid NativeData usage

## Build

```bash
dotnet build samples/NativeData.AnalyzerSmoke/NativeData.AnalyzerSmoke.csproj -c Release
```
