# NativeData.Tests

Unit and integration tests for NativeData runtime and provider behavior.

## Coverage

- SQL generation behavior for repository operations
- SQLite round-trip integration coverage
- Edge cases for parameter normalization and update guards

## Run tests

```bash
dotnet test tests/NativeData.Tests/NativeData.Tests.csproj
```

## Packaging

This project is test-only (`IsPackable=false`).
