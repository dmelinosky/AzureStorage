# CLAUDE.md

## Package version bumps

Whenever you change any `.cs` file, ask the developer whether they want to bump the package version before you finish the task or open a PR. Don't bump it without asking.

- The version is `<Version>` in `src/Gobie74.AzureStorage.csproj`. Merging a new version to `master` publishes it to nuget.org and creates a `v<version>` tag and GitHub release, so a bump is a real release.
- When you ask, suggest a [SemVer](https://semver.org) bump based on the change: **major** for breaking public API changes, **minor** for new backward-compatible features, **patch** for fixes. Say if the change looks test-only or internal and might not need a release.
- If they bump the version, also update `<PackageReleaseNotes>` in the same file to describe the release.

## Project notes

- .NET 10 solution: `AzureStorage.slnx`. Package versions are managed centrally in `Directory.Packages.props`, and shared build settings live in `Directory.Build.props`.
- Build: `dotnet build AzureStorage.slnx -c Release`. It should finish with 0 warnings; StyleCop and the .NET analyzers are enabled.
- Test: `dotnet test --solution AzureStorage.slnx`. The tests are TUnit integration tests that need table storage. Locally, start Azurite with `docker run -p 10002:10002 mcr.microsoft.com/azure-storage/azurite azurite-table --tableHost 0.0.0.0 --skipApiVersionCheck`.
- `master` is protected. Make changes on a branch and open a PR; the GitHub Actions `build` check must pass before it can merge.
