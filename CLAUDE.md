# Project rules for Claude

## What this is

NetCoreMQTTExampleCluster is an example of an MQTT broker cluster. Several MQTT brokers built with
MQTTnet share one Orleans silo. The silo validates every connect, subscribe and publish against a
PostgreSQL database with the TimescaleDB extension, writes an event log and forwards every accepted
message to all other brokers. There is no NuGet package, no installer and no CI in this repository.
The product is the source code: two services and two console tools.

One solution `src/NetCoreMQTTExampleCluster.sln` with 13 projects, every one of them with
`TargetFrameworks` `net10.0;net11.0`. All project names start with `NetCoreMQTTExampleCluster.`, the
prefix is left out below.

Services and tools (`OutputType` `Exe`):

- `Cluster`: the MQTT broker, called "MQTT cluster client" in the readme. `MqttService` hosts the
  MQTTnet server, hands every MQTT event to the repository grain through an Orleans client and
  registers the broker with the silo. Several instances run side by side.
- `SiloHost`: the Orleans silo with ADO.NET clustering and reminders on PostgreSQL, the repositories,
  the `MqttValidator` and the Orleans dashboard on `DashboardPort` (4321) under `/dashboard`.
- `DatabaseSetup`: creates the database from scratch and seeds two users, see "Known quirks".
- `PasswordHashGenerator`: an interactive console tool that prints an ASP.NET Core Identity password
  hash for the `mqttuser` table.

Libraries:

- `Grains.Interfaces`: `IMqttRepositoryGrain` (integer key, always `GlobalConstants.RepositoryGrainId`,
  which is 0), `IMqttClientGrain` (string key, the MQTT client identifier), the serializable
  `Simple*EventArgs` copies of the MQTTnet event arguments and, under `Surrogates/`, the Orleans
  surrogates and converters for `MqttApplicationMessage`, `MqttTopicFilter` and `MqttUserProperty`.
- `Grains`: `MqttRepositoryGrain` holds the broker list, queues the event log and the published
  messages and forwards messages to the other brokers. `MqttClientGrain` loads the MQTT user with its
  black and white lists and delegates every check to `IMqttValidator`.
- `Validation`: `MqttValidator`, the rules for connect, publish and subscribe including the monthly
  byte limit.
- `TopicCheck`: `TopicChecker.Regex`, the match of a topic against a filter with `+` and `#`.
- `Storage`: Dapper repositories on Npgsql, the SQL text in `Statements/`, `DatabaseHelper` for the
  setup and the Orleans PostgreSQL scripts in `OrleansQueries/` as embedded resources.
- `Models`: the configuration classes with `IsValid()`, `GlobalConstants`, `BackgroundServiceBase<T>`
  (the base of `MqttService` and `SiloHostService`, connects the Orleans client and logs the memory
  usage) and small extensions. `Models` references `Storage`, not the other way round.

Tests, all MSTest:

- `TopicCheck.Tests`: wildcard matching with `+`, `#` and both.
- `Validation.Tests`: connect, publish and subscribe validation against `UserMqttRepositoryFake`,
  no database.
- `Grains.Interfaces.Tests`: Orleans serialization round trips of every type passed to a grain.

Repository root: `README.md` (the only user documentation: setup, every setting, the sequence
diagrams), `Changelog.md`, `Updating.md` (one line, see "Updating Orleans and packages"),
`License.txt` (MIT), `images/` (PlantUML sources `*.wsd` with the rendered `*.svg` and `*.png`),
`doc/Example setup/` (outdated, see "Known quirks"), `.claude/skills/github-issue/` and
`.gitattributes`.

## Build

```powershell
dotnet build src/NetCoreMQTTExampleCluster.sln -c Release
```

```powershell
dotnet test src/NetCoreMQTTExampleCluster.sln -c Release
```

- Two target frameworks `net10.0;net11.0` in every project, so building needs the .NET 11 SDK.
  `Microsoft.AspNetCore.Authentication.JwtBearer` in `Cluster` is referenced per target framework
  (10.0.x for `net10.0`, 11.0.x for `net11.0`), keep that split when updating packages.
- There is no `Directory.Build.props`. All build properties are duplicated in the 13 `.csproj`
  files: `TreatWarningsAsErrors`, `NuGetAudit` with `NuGetAuditMode=all`, `LangVersion latest`,
  `ImplicitUsings` and `NoWarn` `NU1803`. Every warning breaks the build, NuGet warnings (`NU****`)
  from restore and vulnerable transitive packages included. A clean build reports zero warnings,
  keep it that way.
- `NU1803` (HTTP source usage during restore) is the one suppressed warning. It is needed because the
  machine has an HTTP feed configured globally next to nuget.org. Fix warnings instead of extending
  that list.
- `GenerateDocumentationFile` is not set, so a missing XML doc comment does not break the build. The
  convention below applies anyway.
- `Nullable` is enabled in every project except `PasswordHashGenerator` and `Validation.Tests`, which
  list `ImplicitUsings` twice instead. Do not switch it on in passing, it needs its own change with a
  build on both frameworks.
- Versions come from GitVersion.MsBuild out of the git tags. Never edit a version property or an
  assembly version by hand.
- The test projects differ in their package set: `Grains.Interfaces.Tests` and `Validation.Tests`
  have no GitVersion.MsBuild, `TopicCheck.Tests` has no `coverlet.collector`.
- Restore needs nuget.org. If a private feed is configured globally on the machine and answers 404
  for public packages, restore fails with `NU1301`. Then build with an explicit source:
  `dotnet build src/NetCoreMQTTExampleCluster.sln --source https://api.nuget.org/v3/index.json`.
- `dotnet test` runs 27 tests per framework, 54 runs in total (TopicCheck 13, Validation 8,
  Grains.Interfaces 6). They need no network and no database. Never claim a test run happened
  without running it.
- The unit tests do not cover the brokers, the grains or the database. A change there needs the end
  to end run in [verification.md](.claude/skills/github-issue/references/verification.md): a
  disposable TimescaleDB in Docker, `DatabaseSetup`, `SiloHost`, two `Cluster` instances and MQTT
  clients. That file is the single description of the end to end run, keep it up to date instead of
  copying it here.

## Running it

- Ports: MQTT 1883 unencrypted and 8883 with TLS on each broker, silo 7101, gateway 7102, dashboard
  4321. The web host of a `Cluster` instance uses the ASP.NET Core default port unless
  `ASPNETCORE_URLS` says otherwise, so a second instance needs its own value.
- Start the `SiloHost` first. A `Cluster` instance gives up after ten failed connection attempts to
  the silo, two seconds apart, and terminates.
- Both services read the environment from `ASPNETCORE_ENVIRONMENT`. Without it the environment is
  `Production`: log level Information, a daily log file in `LogFolderPath` (`C:\log\...` in the
  shipped `appsettings.json`) plus the console. `Development` logs to the console only, at Debug
  level.
- Stop the services with Ctrl+C so that each broker deregisters from the repository grain.

## Code conventions

Follow the surrounding code, it is consistent throughout every file:

- File header comment block with `<copyright file="..." company="Hämmer Electronics">` and a
  `<summary>`, then the file-scoped namespace.
- XML doc comments on every type and every member, private members included. Implementations of an
  interface member carry `<inheritdoc cref="..." />` pointing at that interface.
- `Nullable` (see above), `ImplicitUsings` and `LangVersion latest` are enabled.
- New `using` directives go into the `GlobalUsings.cs` of the respective project, inside the
  existing `#pragma warning disable IDE0065` block, never at the top of a file. The editorconfig
  requires usings inside the namespace (`csharp_using_directive_placement=inside_namespace:warning`),
  which global usings cannot satisfy, that is what the pragma is for. The comment text in that block
  is German because Visual Studio generated it, leave it alone. The only other pragma is `CA1816` in
  `MqttService.StopAsync`, do not add more.
- Logging is Serilog everywhere (`Log.ForContext<T>()`, `ILogger` aliased to `Serilog.ILogger`), not
  `Microsoft.Extensions.Logging`.
- Fields, properties, methods and events are always accessed with `this.` qualification
  (`dotnet_style_qualification_for_*` at severity `warning`).
- `src/.editorconfig` also enforces braces everywhere, no multiple blank lines, a blank line after a
  block, four spaces, CRLF, UTF-8, file scoped namespaces, `System` usings sorted first and `IDE0005`
  as warning. Analyzer warnings are fixed, not silenced.
- Every type that crosses a grain call needs `[GenerateSerializer]` with `[Id(n)]` on each member.
  MQTTnet types cannot carry these attributes, they go through a surrogate struct plus a
  `[RegisterConverter]` converter in `Grains.Interfaces/Surrogates`. A new type or member gets a
  round trip test in `Grains.Interfaces.Tests`.

## Known quirks

Do not silently "clean up" these, they are existing behaviour:

- **`DatabaseSetup` drops the database.** It runs `DROP DATABASE` on the configured name, creates it
  again, enables TimescaleDB, runs the Orleans scripts `Main`, `Clustering` and `Reminders`
  (`Persistence` is embedded but never executed), creates the own tables with hypertables on
  `eventlog` and `publishmessage` and seeds the data. Never point it at a database whose content is
  needed. It ends with `Console.ReadKey()`, which throws with redirected input after the work is done.
- **Seed users.** `Test` with password `test`, no client identifier check, may publish to `a/b` and
  `a/d` and subscribe to `d/e` and `d`. `mqtt-broker-sync` with password `Test`, `IsSyncUser`,
  `ValidateClientId` with the prefix `mqtt-broker-sync`, may publish and subscribe to `#`. The readme
  says `e` instead of `d` for the subscription, `DatabaseSetup/Program.cs` is authoritative.
- **The sync user prevents loops.** A message published by a user with `IsSyncUser` is validated and
  stored, but not forwarded again. Every broker's `BrokerConnectionSettings` logs in as that user,
  and the user must not be used for anything else.
- **One repository grain for all brokers.** Key 0, `[Reentrant]`, `ProceedPublish` additionally
  `[AlwaysInterleave]`. The broker list lives only in memory. The brokers register again every
  `ServiceDelayInMilliseconds` (30 s), which restores the list after a silo restart or a grain
  deactivation. Event log entries and published messages are queued and written every 20 seconds
  (the first time after 5) and on deactivation, a silo crash loses the queue.
- **Forwarding opens one MQTT connection per message and broker.** `PublishMessageToBroker` connects,
  publishes and disconnects for every message, fire and forget via `.Ignore()`. The payload is
  decoded as UTF-8 and encoded again, so a payload that is not valid UTF-8 arrives altered on the
  other brokers. Together with the single repository grain this is the known scaling limit,
  discussed in issue #1.
- **The client grain caches the user.** `MqttClientGrain` loads the user and its lists once per
  activation. Changes in the database take effect after the grain is deactivated (Orleans collects
  idle grains after 15 minutes by default) or the silo restarts.
- **The monthly byte limit lives in memory.** `ThrottleUser` plus `MonthlyByteLimit` count into a
  static `MemoryCache` per silo process that expires at the end of the month. A silo restart resets
  the count.
- **Rejections carry reason codes.** A rejected connect answers `BadUserNameOrPassword`, a rejected
  subscription a SUBACK with `NotAuthorized`, a rejected publish a PUBACK with `NotAuthorized` (MQTT 5
  only, MQTT 3.1.1 has no reason code there). If the silo is unreachable, connect answers
  `ServerUnavailable`, subscribe and publish `UnspecifiedError`.
- **`ValidateClientIdPrefix`** (SiloHost setting, `false` in code, `true` in the shipped
  `appsettings.json`) only affects users with `ValidateClientId` and a `ClientIdPrefix`. A user with
  `ValidateClientId` and no prefix always has to match `ClientId` exactly.
- **Plain text password in the debug log.** `MqttValidator` destructures the connect context
  (`{@Context}`) at Debug level, the password included. Debug is only active in `Development`. This
  was left that way deliberately on 2026-11-25, do not change it unless asked. The event log table
  no longer contains the password.
- **Example certificate.** `certificate.pfx` (with `cert.pem` and `key.pem`) is self-signed for
  `localhost` and valid until 2036-11-25. Its password `test` is hard-coded in
  `MqttService.StartupService`, TLS is fixed to 1.2. Because it is self-signed, the sync connection
  between brokers has to use the unencrypted port with `UseTls: false` in local tests.
- **The configuration section is the assembly name.** Both services bind the section
  `NetCoreMQTTExampleCluster.Cluster` or `NetCoreMQTTExampleCluster.SiloHost` from
  `Assembly.GetName()`. Renaming an assembly breaks the configuration. `IsValid()` throws a
  `ConfigurationException` on the first invalid value, `JsonWebTokenConfigurationKey` needs at least
  32 characters. The shipped key is a placeholder, kept deliberately. `appsettings.Development.json`
  is identical to `appsettings.json` in both services.
- **Blazor leftovers in `Cluster`.** `App.razor` and `_Imports.razor` reference MatBlazor, AutoMapper,
  BlazorPro and `IVU.Portal.*` namespaces that do not exist here. The project uses
  `Microsoft.NET.Sdk`, not the Razor SDK, so these files are never compiled. `Startup` still
  registers MVC, Razor Pages, server side Blazor, SignalR and JWT authentication and serves an empty
  `wwwroot`, but maps no endpoint. Leave it alone unless the task is to remove it.
- **Two copies of the Orleans SQL scripts.** `Storage/OrleansQueries/PostgreSQL-*.sql` are embedded
  and executed by `DatabaseSetup`. `SiloHost/OrleansAdoNetContent/` holds the scripts for MySQL,
  Oracle, PostgreSQL and SQL Server for reference and is used by nothing. Its PostgreSQL files are
  identical to the embedded ones (Orleans 10.4.2).
- **`doc/Example setup/` is outdated.** The two JSON files date from the initial commit in 2020: flat
  JSON without the section name, no `LogFolderPath`, no JWT key, and the user `Test` as sync user.
  Nothing loads them. Use verification.md for a two broker setup.
- **AppVeyor badge without CI in the repository.** `README.md` links an AppVeyor build that is
  configured outside of this repository and has no .NET 11, left that way deliberately. There is no
  `.github` folder and no pipeline file here.
- **`src/NetCoreMQTTExampleCluster.sln.DotSettings`** is tracked and holds nothing but a ReSharper user
  dictionary. Leave it alone.
- **`src/Delete-BIN-OBJ-Folders.bat` ends with `pause`.** Non interactively it needs its standard
  input redirected from `nul`.
- **`.gitattributes` sets `* text=auto`** and every rule of the Visual Studio template below it is
  commented out. Any binary file added later needs its own rule.

## Updating Orleans and packages

- Keep every `Microsoft.Orleans.*` package on the same version (10.4.2 at the moment), and `MQTTnet`
  and `MQTTnet.Server` on the same version.
- `Updating.md` says: when Orleans is updated, take the ADO.NET scripts of the new version from
  https://dotnet.github.io/orleans/Documentation/clusters_and_clients/configuration_guide/adonet_configuration.html
  and update both script folders (`Storage/OrleansQueries` and `SiloHost/OrleansAdoNetContent`).
- Orleans was migrated from 3.7 to 10.4.2 on 2026-11-25. An Orleans update needs the end to end run,
  the unit tests only cover serialization.

## Versions

- The tags `1.0.0` to `1.5.0` are lightweight tags from 2020, three parts, no `v` prefix.
  `Changelog.md` uses four parts (`1.4.0.0`) and ends at 1.4.0.0, the tag `1.5.0` has no entry. There
  are no GitHub releases. Everything since 2026-11-25 is untagged, `git describe --tags` answers with
  `1.5.0-<n>-g<sha>`.
- New entries go at the top of `Changelog.md` in the existing format, for example
  `* **Version 1.4.0.0 (2020-06-29)** : Updated nuget packages, ...`.
- Do not create a tag, a changelog version or a GitHub release unless asked.

## Git

- Include the full GitHub pull request or issue URL in the body of each related commit on a `Refs:`
  line. Use the [GitHub issue skill](.claude/skills/github-issue/SKILL.md) for issue work and closeout.
- **Never amend a commit.** No `git commit --amend`, not for a typo in the message, not to add a
  forgotten file, not even when the commit is still local. Write a follow-up commit instead. The
  versions come from tags on exact commits, an amended commit leaves its tag pointing at a commit
  that no longer exists in the branch.

## Writing style

- Commit messages are written **in English only**: short, precise subject line, explanatory body
  when needed.
- Code comments and comments in project files such as `.csproj` are **always English**, regardless
  of the language used in the conversation.
- **No em dashes or en dashes** (`—`, `–`), neither in prose, commit messages, code comments nor
  documentation. Use a regular hyphen, comma, colon, parentheses or a separate sentence.
- German texts (documentation, chat replies) always use real umlauts and ß, never ASCII
  transliterations such as `ae`, `oe`, `ue` or `ss`. Identifiers, file names and configuration keys
  stay unchanged where umlauts are technically undesirable.
