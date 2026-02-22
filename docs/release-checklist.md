---
title: Release Checklist
nav_order: 3
---

# NativeData Release Checklist

Use this checklist as a go/no-go gate before publishing any NativeData release.

## How to Use

- Preferred automation: run `./scripts/release.ps1 -Push` from `main`.
- Alternative automation: trigger `.github/workflows/release-on-demand.yml`.
- Copy this checklist into the release PR description.
- Or open a PR with the built-in release template: `?template=release.md`.
- Mark each item as complete with evidence (link to PR/test run/artifact).
- If an item is not applicable, mark it `N/A` with a short reason.
- CI baseline workflow for these gates: `.github/workflows/ci.yml`.

## Universal Gates (All Releases)

### Build and Test

- [ ] `dotnet build NativeData.slnx -warnaserror` succeeds
- [ ] `dotnet test NativeData.slnx` succeeds
- [ ] AOT smoke publish succeeds:
  - [ ] `dotnet publish samples/NativeData.AotSmoke/NativeData.AotSmoke.csproj -c Release -r win-x64`

### API and Compatibility

- [ ] Public API changes reviewed for semver impact
- [ ] Breaking changes documented (or explicitly none)
- [ ] New/updated public APIs documented in docs and XML comments

### Packaging and Metadata

- [ ] Package versions updated correctly
- [ ] Package metadata verified (description, tags, repository URL)
- [ ] Release notes/changelog entry written (use `docs/release-notes-template.md`)

### Documentation

- [ ] README updated for user-visible behavior changes
- [ ] Roadmap/status doc updated if milestone scope changed
- [ ] New diagnostics or provider behaviors documented

## Milestone-Specific Gates

## v0.2.0 — Foundation Hardening

- [ ] SQL generation edge-case tests added (nulls, parameter names, update behavior)
- [ ] Contract docs for `Abstractions` and `Core` reviewed
- [ ] CI pipeline includes build/test/AOT smoke jobs

## v0.3.0 — Generated Mapping

- [ ] At least one entity uses generated `IEntityMap<T>` end-to-end
- [ ] Generator output deterministic across clean/repeat builds
- [ ] Unsupported entity shapes produce clear diagnostics
- [ ] Benchmark evidence captured for generated vs manual mapping

## v0.4.0 — Analyzer Expansion

- [ ] Analyzer rule IDs/messages/categories finalized
- [ ] Each analyzer rule has positive + negative tests
- [ ] Analyzer docs include remediation guidance per rule
- [ ] Release tracking metadata for analyzer package validated

## v0.5.0 — Second Provider Validation

- [ ] Postgres provider package compiles and passes tests
- [ ] Shared behavioral tests pass for SQLite and Postgres
- [ ] Provider setup docs verified by running sample/config steps
- [ ] AOT smoke publish still passes with second provider referenced

## v1.0.0 — Production Baseline

- [ ] API freeze approved
- [ ] Two providers are release-quality with compatibility matrix documented
- [ ] Generator + analyzer packs are production-ready and documented
- [ ] Final release checklist and sign-off captured by maintainers

## Optional Operational Gates

- [ ] Benchmark trend check (no major regression from previous release)
- [ ] Basic dependency audit complete
- [ ] NuGet package install smoke test in a clean sample project

## Final Sign-Off

- Release version: `__________`
- Milestone: `__________`
- Release owner: `__________`
- Date: `__________`
- Final decision: `GO / NO-GO`
