# Contributing to NativeData

Thanks for helping build NativeData.

## Development Setup

1. Install .NET 10 SDK.
2. Restore and build:

```bash
dotnet restore NativeData.slnx
dotnet build NativeData.slnx -warnaserror
```

3. Run tests:

```bash
dotnet test NativeData.slnx
```

4. Run AOT smoke publish before major runtime/provider changes:

```bash
dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true
```

## Pull Requests

- Keep changes focused and minimal.
- Add or update tests for behavior changes.
- Update docs when user-facing behavior changes.
- Use the standard PR template.
- For release PRs, open with `?template=release.md` and complete the release checklist.

## CI

- Main CI workflow: `.github/workflows/ci.yml`
- Run history: `https://github.com/skidsh/NativeData/actions/workflows/ci.yml`
- CI gates mirror local release checks:
  - `dotnet build NativeData.slnx -warnaserror`
  - `dotnet test NativeData.slnx`
  - `dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true`

## Coding Guidelines

- Preserve NativeAOT/trimming-safe design.
- Prefer compile-time/static patterns over runtime reflection.
- Avoid dynamic loading/runtime code generation patterns in core runtime paths.

## Documentation and Planning

- Project status/roadmap: `docs/status-and-roadmap.md`
- Release gates: `docs/release-checklist.md`

## Release Process

- Merge PRs into `main`.
- Run automated local release from `main`:

```powershell
./scripts/release.ps1 -Push
```

- Or trigger on-demand release workflow: `.github/workflows/release-on-demand.yml`.
- Versioning is automated: the script computes the next patch version from the latest `vX.Y.Z` git tag.

## License

By contributing, you agree your contributions are licensed under the MIT License in this repository.
