# NativeData.Benchmarks

Benchmarks for NativeData generated mapping vs manual mapping hot paths.

## Run

```bash
dotnet run -c Release --project benchmarks/NativeData.Benchmarks/NativeData.Benchmarks.csproj
```

## Scope

- Insert parameter binding (`BuildInsertParameters`)
- Update parameter binding (`BuildUpdateParameters`)
- Materialization from `IDataRecord` (`Materialize`)
