# NativeData.Abstractions

Defines the core contracts and primitives used across NativeData runtime and providers.

## Contents

- Repository and execution contracts (`IRepository<T>`, `ICommandExecutor`, `IDbConnectionFactory`)
- Mapping and SQL dialect abstractions (`IEntityMap<T>`, `ISqlDialect`)
- Shared primitives (`SqlParameterValue`, `NativeDataEntityAttribute`)

## Build

```bash
dotnet build src/NativeData.Abstractions/NativeData.Abstractions.csproj
```

## Packaging

This project is packable and published as a NuGet package.
