# NativeData.AotSmoke

Minimal sample app used to validate NativeAOT + trimming compatibility.

## What it does

- Creates a SQLite database
- Creates table schema
- Inserts and reads an entity via NativeData repository flow

## Build

```bash
dotnet build samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj
```

## AOT smoke publish

```bash
dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -p:PublishTrimmed=true
```

## Packaging

This project is sample-only and marked `IsPackable=false`.
