# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

`DSoft.ServiceRegistrar` — a dependency injection and service registration library (`Services`, `DiscoverableServiceAttribute`, `SingletonServiceAttribute`). One NuGet package plus test projects:

- `DSoft.ServiceRegistrar/` — the library, the only package that is published.
- `TestContracts/`, `TestImplementations/` — `netstandard2.0` helpers for the harness.
- `TestHarnessTwo/` — console harness (`net10.0;net11.0`); it waits for Enter at the end, so run it with `</dev/null` in scripts. There are no automated tests.

## Build

```bash
dotnet restore ServiceRegistra.sln
dotnet build ServiceRegistra.sln -c Release
dotnet TestHarnessTwo/bin/Release/net10.0/TestHarnessTwo.dll </dev/null
```

- The library multi-targets `netstandard2.0;net6.0;net8.0;net9.0;net10.0;net11.0` and has no package dependencies. Build with the .NET 10 and .NET 11 SDKs (`global.json` rolls forward to the latest major).
- `GeneratePackageOnBuild=true` (`Directory.Build.props`), so every build produces `.nupkg` files (the library's has `README.md` and `DSIcon.png`). Release builds add SourceLink.
- Assemblies are strong-name signed with `DSoft.ServiceRegistrar/DSoft.snk`; do not remove it.

## Branching

- `development` is the working branch: branch from it and open pull requests into it. Never commit or open a pull request directly against `main`.
- `main` is the release branch. Every push to `main` runs `release.yml`, which publishes the package to nuget.org and creates a GitHub release, so only merge `development` into `main` when a release is intended, and only when asked to release.

## CI

GitHub Actions (`.github/workflows/`) on `windows-latest`:

- `ci.yml` builds Release for pull requests into `main` and `development`; it publishes nothing.
- `release.yml` runs on every push to `main` (changes only to Markdown or workflow files are skipped; run it by hand to test a workflow change). It builds Release as `1.3.<yyMM>.<run number>` plus `RELEASE_SUFFIX` (empty for a stable version), pushes `DSoft.ServiceRegistrar` to nuget.org with Trusted Publishing (OIDC, `NUGET_USER` secret, `nuget` environment), then tags the commit `v<version>` and creates a GitHub release with the package attached.
