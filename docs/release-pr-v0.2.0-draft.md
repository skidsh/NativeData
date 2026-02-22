## Release PR

Release version: `0.2.0`

Milestone: `v0.2.0`

## Scope Summary

- Hardened SQL repository behavior and expanded edge-case test coverage.
- Enabled XML documentation output and added public API XML docs for `Abstractions` and `Core`.
- Added packaging metadata baseline for packable projects.
- Added CI workflow to enforce build/test/AOT smoke gates.
- Synced maintainer/contributor documentation with release and CI process.

## Linked Artifacts

- Changelog entry (from docs/release-notes-template.md): docs/release-notes-v0.2.0.md
- Relevant milestone/issues: `(add links)`
- CI run(s): `(add workflow run URL)`

## Universal Gates

### Build and Test

- [x] `dotnet build NativeData.slnx -warnaserror` succeeds
- [x] `dotnet test NativeData.slnx` succeeds
- [x] AOT smoke publish succeeds:
  - [x] `dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true`

### API and Compatibility

- [x] Public API changes reviewed for semver impact
- [x] Breaking changes documented (or explicitly none)
- [x] New/updated public APIs documented in docs and XML comments

Notes:
- Breaking changes: `None`.
- Update behavior now throws `InvalidOperationException` when an entity map provides no non-key update assignments (prevents invalid SQL generation).

### Packaging and Metadata

- [x] Package versions updated correctly
- [x] Package metadata verified (description, tags, repository URL)
- [x] Release notes/changelog entry written

Notes:
- Metadata baseline centralized in Directory.Build.props for packable projects.
- Added release notes template at docs/release-notes-template.md.

### Documentation

- [x] README updated for user-visible behavior changes
- [x] Roadmap/status doc updated if milestone scope changed
- [x] New diagnostics or provider behaviors documented

## Milestone-Specific Gates

### v0.2.0 — Foundation Hardening

- [x] SQL generation edge-case tests added (nulls, parameter names, update behavior)
- [x] Contract docs for `Abstractions` and `Core` reviewed
- [x] CI pipeline includes build/test/AOT smoke jobs

## Optional Operational Gates

- [ ] Benchmark trend check (no major regression from previous release)
- [ ] Basic dependency audit complete
- [ ] NuGet package install smoke test in a clean sample project

## Final Sign-Off

- Release owner: `Kyle.Keller`
- Date: `2026-02-21`
- Final decision: `GO`

## Included Changes (Quick Index)

- SQL behavior/tests:
  - src/NativeData.Core/SqlRepository.cs
  - tests/NativeData.Tests/UnitTest1.cs
- XML docs and doc file generation:
  - src/NativeData.Abstractions/*.cs
  - src/NativeData.Abstractions/NativeData.Abstractions.csproj
  - src/NativeData.Core/DbCommandExecutor.cs
  - src/NativeData.Core/DefaultSqlDialect.cs
  - src/NativeData.Core/SqlRepository.cs
  - src/NativeData.Core/NativeData.Core.csproj
- Packaging metadata baseline:
  - Directory.Build.props
  - docs/release-notes-template.md
- CI/process/docs:
  - .github/workflows/ci.yml
  - .github/PULL_REQUEST_TEMPLATE/release.md
  - docs/release-checklist.md
  - docs/status-and-roadmap.md
  - CONTRIBUTING.md
  - README.md
