# NativeData

[![CI](https://github.com/skidsh/NativeData/actions/workflows/ci.yml/badge.svg)](https://github.com/skidsh/NativeData/actions/workflows/ci.yml)

NativeData is an AOT-first ORM foundation for .NET 10.

## Documentation

- [Project status and roadmap](docs/status-and-roadmap.md)
- [Release checklist](docs/release-checklist.md)
- [Contributing guide](CONTRIBUTING.md)

## Maintainer Notes

- For release PRs, open with GitHub template: `?template=release.md`
- Use [.github/PULL_REQUEST_TEMPLATE/release.md](.github/PULL_REQUEST_TEMPLATE/release.md) with [docs/release-checklist.md](docs/release-checklist.md) as go/no-go gates

## Current MVP

- Repository-style API (`IRepository<T>`)
- Provider-agnostic runtime contracts (`ICommandExecutor`, `ISqlDialect`, `IEntityMap<T>`)
- Minimal SQL repository implementation (`SqlRepository<T>`)
- ADO.NET command execution adapter (`DbCommandExecutor`)
- SQLite provider package (`NativeData.Sqlite`) with `SqliteConnectionFactory` and `SqliteSqlDialect`
- Source generator skeleton for `[NativeDataEntity]`
- Analyzer skeleton for trim safety checks

## Explicitly out of scope in MVP

- Migrations
- Change tracking
- LINQ provider/translation

## Build

```bash
dotnet restore NativeData.slnx
dotnet build NativeData.slnx -warnaserror
dotnet test NativeData.slnx
```

## AOT smoke publish

```bash
dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true
```

## Release automation

Release from `main` with one command (auto-calculates next patch version from git tags):

```powershell
./scripts/release.ps1 -Push
```

Or run the on-demand GitHub workflow:

- `.github/workflows/release-on-demand.yml`

## License

NativeData is open source under the MIT License. See [LICENSE](LICENSE).