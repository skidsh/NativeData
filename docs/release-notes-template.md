# NativeData Release Notes Template

Use this template when drafting release notes and changelog content for any package release.

## Release

- Version: `x.y.z`
- Date: `yyyy-mm-dd`
- Milestone: `v0.2.0 | v0.3.0 | v0.4.0 | v0.5.0 | v1.0.0`

## Highlights

- Short summary of the most important user-visible changes.

## Added

- New features, APIs, providers, diagnostics, or tooling.

## Changed

- Behavior changes, performance changes, and compatibility updates.

## Fixed

- Bug fixes and reliability improvements.

## Documentation

- README, roadmap, and guidance updates.

## Breaking Changes

- List breaking changes, migration guidance, and impacted users.
- If none, write: `None`.

## Validation

- `dotnet build NativeData.slnx -warnaserror`
- `dotnet test NativeData.slnx`
- AOT smoke publish command and result

## Package Metadata Checklist

- Description and tags verified
- Repository URL verified
- License metadata verified
- Version updated correctly

## References

- Release checklist: `docs/release-checklist.md`
- Release PR template: `.github/PULL_REQUEST_TEMPLATE/release.md`
