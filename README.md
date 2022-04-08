# MEL.Flex

## What is this?

MEL.Flex (FSharp Logging EXtensions for [Microsoft.Extensions.Logging](https://docs.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)) adds the ability to use [string interpolation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/interpolated-strings) for strongly typed strings while getting the benefits of [structured logging](https://stackify.com/what-is-structured-logging-and-why-developers-need-it/) by converting the interpolated string into a [message template](https://messagetemplates.org/#:~:text=A%20message%20template%20is%20a,it%20into%20human%2Dfriendly%20text.) transparently.

## Why does this exist?

[Structured logging](https://stackify.com/what-is-structured-logging-and-why-developers-need-it/) with [message templates](https://messagetemplates.org/#:~:text=A%20message%20template%20is%20a,it%20into%20human%2Dfriendly%20text.) are great, except they do one a few problems.

1. They are (usually) positional. If I have the following code:

    ```fsharp
    let userName = "KirkJ1701"
    logger.LogWarning("Some user: {UserName} has logged in", userName)
    ```

    if I wanted to add an IpAddress

    ```fsharp
    let userName = "KirkJ1701"
    let ipAddress = "KirkJ1701"
    logger.LogWarning("Some user: {UserName} has logged in from {IpAddress}", ipAddress, userName)
    ```

    I can easily mess up the arguments. This of course looks easy but gets more difficult as you add more logs to structure or rearrange your logs.

2. Normalizing your keys with [semantic convetions](https://github.com/open-telemetry/opentelemetry-specification/blob/main/specification/trace/semantic_conventions/README.md) makes it easier for you to search across your logs.

    ```fsharp
    let userName = "SpockV"
    logger.LogWarning("Some user: {user_name} has logged in", userName)
    ```

    Adding this log to our application logs the `user_name` but earlier we were logging a `UserName`. If I needed to search across these logs, I would need to know each potential variation of that key name.  (Yes some tools allow you to do that mapping on their side but not all).

## How do I use it?

### Tupled Arguments

One of the currently supported ways is to use tuples. Taking an example above:

```fsharp
// Some file that containts your normalized names
module LoggerKeyConsts =
    let [<Literal>] UserName = "UserName"

// .. Some function

let userName = "SpockV"
logger.LogFWarning($"Some user: {(LoggerKeyConsts.UserName, userName)} has logged in")
```

The important changes are:

1. `LogWarning` -> `LogFWarning`
2. `$` in front of the string
3. `{Username}` became `{(LoggerKeyConsts.UserName, userName)}`
    - The latter part of this syntax is a tuple with two values.

Slightly better would be to create helper functions that generate the tuples:

```fsharp
module LogConsts =
    let [<Literal>] ``user.name`` = "user.name"
    let inline userName (s : string) = struct (``user.name``, s)

// .. Some function
let userName = "SpockV"
logger.LogFWarning($"Some user: {LogConsts.userName userName} has logged in")
```

This way, you could apply any type safety or normalization to your log data.

## Caveats

- Unfortunately F# does not yet support [DefaultInterpolatedStringHandler](https://github.com/fsharp/fslang-suggestions/issues/1108) which means you will still take the interpolated creation hit. However, this library does implement it's own [log formatter](https://github.com/TheAngryByrd/MEL.Flex/blob/19056afce7b39d507f7d99aa10cd36fbdd623f27/src/MEL.Flex/MEL.Flex.fs#L38) which allows for lazy construction of the interpolated string -> message template until it is required or possibly not at all if the [LogLevel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0#set-log-level-by-command-line-environment-variables-and-other-configuration) configuration is set to a higher threadshold than the log statement.
---

## Builds

GitHub Actions |
:---: |
[![GitHub Actions](https://github.com/TheAngryByrd/MEL.Flex/workflows/Build%20master/badge.svg)](https://github.com/TheAngryByrd/MEL.Flex/actions?query=branch%3Amaster) |
[![Build History](https://buildstats.info/github/chart/TheAngryByrd/MEL.Flex)](https://github.com/TheAngryByrd/MEL.Flex/actions?query=branch%3Amaster) |

## NuGet

Package | Stable | Prerelease
--- | --- | ---
MEL.Flex | [![NuGet Badge](https://buildstats.info/nuget/MEL.Flex)](https://www.nuget.org/packages/MEL.Flex/) | [![NuGet Badge](https://buildstats.info/nuget/MEL.Flex?includePreReleases=true)](https://www.nuget.org/packages/MEL.Flex/)

---

### Developing

Make sure the following **requirements** are installed on your system:

- [dotnet SDK](https://www.microsoft.com/net/download/core) 3.0 or higher
- [Mono](http://www.mono-project.com/) if you're on Linux or macOS.

or

- [VSCode Dev Container](https://code.visualstudio.com/docs/remote/containers)


---

### Environment Variables

- `CONFIGURATION` will set the [configuration](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build?tabs=netcore2x#options) of the dotnet commands.  If not set, it will default to Release.
  - `CONFIGURATION=Debug ./build.sh` will result in `-c` additions to commands such as in `dotnet build -c Debug`
- `GITHUB_TOKEN` will be used to upload release notes and Nuget packages to GitHub.
  - Be sure to set this before releasing
- `DISABLE_COVERAGE` Will disable running code coverage metrics.  AltCover can have [severe performance degradation](https://github.com/SteveGilham/altcover/issues/57) so it's worth disabling when looking to do a quicker feedback loop.
  - `DISABLE_COVERAGE=1 ./build.sh`


---

### Building


```sh
> build.cmd <optional buildtarget> // on windows
$ ./build.sh  <optional buildtarget>// on unix
```

The bin of your library should look similar to:

```
$ tree src/MEL.Flex/bin/
src/MEL.Flex/bin/
└── Debug
    └── net50
        ├── MEL.Flex.deps.json
        ├── MEL.Flex.dll
        ├── MEL.Flex.pdb
        └── MEL.Flex.xml

```

---

### Build Targets

- `Clean` - Cleans artifact and temp directories.
- `DotnetRestore` - Runs [dotnet restore](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-restore?tabs=netcore2x) on the [solution file](https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2019).
- [`DotnetBuild`](#Building) - Runs [dotnet build](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build?tabs=netcore2x) on the [solution file](https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2019).
- `DotnetTest` - Runs [dotnet test](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-test?tabs=netcore21) on the [solution file](https://docs.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file?view=vs-2019).
- `GenerateCoverageReport` - Code coverage is run during `DotnetTest` and this generates a report via [ReportGenerator](https://github.com/danielpalme/ReportGenerator).
- `WatchTests` - Runs [dotnet watch](https://docs.microsoft.com/en-us/aspnet/core/tutorials/dotnet-watch?view=aspnetcore-3.0) with the test projects. Useful for rapid feedback loops.
- `GenerateAssemblyInfo` - Generates [AssemblyInfo](https://docs.microsoft.com/en-us/dotnet/api/microsoft.visualbasic.applicationservices.assemblyinfo?view=netframework-4.8) for libraries.
- `DotnetPack` - Runs [dotnet pack](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack). This includes running [Source Link](https://github.com/dotnet/sourcelink).
- `SourceLinkTest` - Runs a Source Link test tool to verify Source Links were properly generated.
- `PublishToNuGet` - Publishes the NuGet packages generated in `DotnetPack` to NuGet via [paket push](https://fsprojects.github.io/Paket/paket-push.html).
- `GitRelease` - Creates a commit message with the [Release Notes](https://fake.build/apidocs/v5/fake-core-releasenotes.html) and a git tag via the version in the `Release Notes`.
- `GitHubRelease` - Publishes a [GitHub Release](https://help.github.com/en/articles/creating-releases) with the Release Notes and any NuGet packages.
- `FormatCode` - Runs [Fantomas](https://github.com/fsprojects/fantomas) on the solution file.
- `BuildDocs` - Generates Documentation from `docsSrc` and the [XML Documentation Comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/) from your libraries in `src`.
- `WatchDocs` - Generates documentation and starts a webserver locally.  It will rebuild and hot reload if it detects any changes made to `docsSrc` files, libraries in `src`, or the `docsTool` itself.
- `ReleaseDocs` - Will stage, commit, and push docs generated in the `BuildDocs` target.
- [`Release`](#Releasing) - Task that runs all release type tasks such as `PublishToNuGet`, `GitRelease`, `ReleaseDocs`, and `GitHubRelease`. Make sure to read [Releasing](#Releasing) to setup your environment correctly for releases.
---


### Releasing

- [Start a git repo with a remote](https://help.github.com/articles/adding-an-existing-project-to-github-using-the-command-line/)

```sh
git add .
git commit -m "Scaffold"
git remote add origin https://github.com/user/MEL.Flex.git
git push -u origin master
```

- [Create your NuGeT API key](https://docs.microsoft.com/en-us/nuget/nuget-org/publish-a-package#create-api-keys)
    - [Add your NuGet API key to paket](https://fsprojects.github.io/Paket/paket-config.html#Adding-a-NuGet-API-key)

    ```sh
    paket config add-token "https://www.nuget.org" 4003d786-cc37-4004-bfdf-c4f3e8ef9b3a
    ```

    - or set the environment variable `NUGET_TOKEN` to your key


- [Create a GitHub OAuth Token](https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/)
  - You can then set the environment variable `GITHUB_TOKEN` to upload release notes and artifacts to github
  - Otherwise it will fallback to username/password

- Then update the `CHANGELOG.md` with an "Unreleased" section containing release notes for this version, in [KeepAChangelog](https://keepachangelog.com/en/1.1.0/) format.

NOTE: Its highly recommend to add a link to the Pull Request next to the release note that it affects. The reason for this is when the `RELEASE` target is run, it will add these new notes into the body of git commit. GitHub will notice the links and will update the Pull Request with what commit referenced it saying ["added a commit that referenced this pull request"](https://github.com/TheAngryByrd/MiniScaffold/pull/179#ref-commit-837ad59). Since the build script automates the commit message, it will say "Bump Version to x.y.z". The benefit of this is when users goto a Pull Request, it will be clear when and which version those code changes released. Also when reading the `CHANGELOG`, if someone is curious about how or why those changes were made, they can easily discover the work and discussions.

Here's an example of adding an "Unreleased" section to a `CHANGELOG.md` with a `0.1.0` section already released.

```markdown
## [Unreleased]

### Added
- Does cool stuff!

### Fixed
- Fixes that silly oversight

## [0.1.0] - 2017-03-17
First release

### Added
- This release already has lots of features

[Unreleased]: https://github.com/user/MEL.Flex.git/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/user/MEL.Flex.git/releases/tag/v0.1.0
```

- You can then use the `Release` target, specifying the version number either in the `RELEASE_VERSION` environment
  variable, or else as a parameter after the target name.  This will:
  - update `CHANGELOG.md`, moving changes from the `Unreleased` section into a new `0.2.0` section
    - if there were any prerelease versions of 0.2.0 in the changelog, it will also collect their changes into the final 0.2.0 entry
  - make a commit bumping the version:  `Bump version to 0.2.0` and adds the new changelog section to the commit's body
  - publish the package to NuGet
  - push a git tag
  - create a GitHub release for that git tag

macOS/Linux Parameter:

```sh
./build.sh Release 0.2.0
```

macOS/Linux Environment Variable:

```sh
RELEASE_VERSION=0.2.0 ./build.sh Release
```


