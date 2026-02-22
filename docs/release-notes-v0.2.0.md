# NativeData 0.2.0 Release Notes

## Release

- Version: `0.2.0`
- Date: `2026-02-21`
- Milestone: `v0.2.0`

## Highlights

- Completed v0.2.0 foundation hardening scope for quality, documentation, packaging metadata, and CI enforcement.

## Added

- CI workflow at .github/workflows/ci.yml with required gates:
  - `dotnet build NativeData.slnx -warnaserror`
  - `dotnet test NativeData.slnx`
  - `dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true`
- Release notes baseline template: docs/release-notes-template.md
- SQL edge-case tests for:
  - parameter prefix normalization (`@`, `:`, `$`)
  - key-only update guard behavior
  - null parameter values
  - whitespace `whereClause` handling

## Changed

- Enabled XML documentation output in:
  - src/NativeData.Abstractions/NativeData.Abstractions.csproj
  - src/NativeData.Core/NativeData.Core.csproj
- Added XML comments across public APIs in NativeData.Abstractions and NativeData.Core.
- Added centralized package metadata baseline for packable projects in Directory.Build.props.
- Aligned documentation and release process references across README, contributing guide, and release checklist.

## Fixed

- Prevented invalid SQL generation in update operations by throwing InvalidOperationException when an entity map provides no non-key update assignments.

## Documentation

- Added CI status badge and updated build commands in README.md.
- Added CI section and gate details in CONTRIBUTING.md.
- Updated v0.2 progress in docs/status-and-roadmap.md.
- Updated release checklist and release PR template to reference release notes template and CI baseline workflow.

## Breaking Changes

- None.

## Validation

- `dotnet build NativeData.slnx -warnaserror`: passed
- `dotnet test NativeData.slnx`: passed
- AOT smoke publish command:
  - `dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64 -p:PublishAot=true -p:PublishTrimmed=true`
  - status: passed

## Package Metadata Checklist

- Description and tags verified
- Repository URL verified
- License metadata verified
- Version updated correctly

## References

- Release checklist: docs/release-checklist.md
- Release PR template: .github/PULL_REQUEST_TEMPLATE/release.md
- Release PR draft: docs/release-pr-v0.2.0-draft.md
