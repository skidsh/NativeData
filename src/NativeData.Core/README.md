# NativeData.Core

Provides the default runtime implementation for the NativeData abstraction layer.

## Contents

- `SqlRepository<T>` CRUD SQL behavior
- `DbCommandExecutor` ADO.NET command/query execution
- `DefaultSqlDialect` baseline SQL identifier/parameter formatting

## Build

```bash
dotnet build src/NativeData.Core/NativeData.Core.csproj
```

## Packaging

This project is packable and published as a NuGet package.
