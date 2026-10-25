# Acderator

Acderator is a rhythm game made with Unity.

## Repository layout

```text
Client/  Unity 6000.3 project
Server/  ASP.NET Core 8 + MagicOnion server
Shared/  Acderator.Shared RPC contracts used as a .NET library and Unity local package
```

The previous Rust/Axum backend from `AcderatorServer` has been migrated into this repository. The server keeps the existing MySQL schema and MessagePack-based request/response models while exposing them as typed MagicOnion services.

## Getting Started

### Requirements

- Unity 6000.3.0f1
- .NET SDK 8.x

You can download the Unity Editor from the [official Download Archive](https://unity.com/releases/editor/archive).

### Installation

Coming soon.

### Building

Open the `Client` directory with Unity 6000.3.0f1 and build from Unity Editor.

NuGetForUnity restores `MagicOnion.Client`, `Grpc.Net.Client`, and `System.IO.Pipelines` from `Client/Assets/packages.config`. Unity Package Manager restores the MagicOnion Unity integration, YetAnotherHttpHandler, and the local `Shared` package.

Set the gRPC endpoint in the existing `NetworkConfig` asset. Development over cleartext HTTP uses HTTP/2 (h2c); production should use HTTPS.

## Development

Run local checks from the `Client` directory.

```bash
dotnet run --project CodingRuleChecker.csproj
dotnet format whitespace --folder --verify-no-changes --include Assets/Acderator
```

The coding rule checker validates C# files under `Assets/Acderator`,   
excluding generated master data under `Assets/Acderator/Scripts/Intense/Master`.  

It checks:

- unused `using` directives
- ABC ordering for `using` directives
- `.editorconfig` warning diagnostics
- C# files must not end with a final newline

To apply supported automatic fixes:

```bash
dotnet run --project CodingRuleChecker.csproj -- --fix
```

Generated `bin` and `obj` directories should not be committed.

### Git hooks

Enable the repository's `pre-push` hook to run the same lint checks as CI before every `git push`:

```bash
git config core.hooksPath .githooks
```

Run once per clone. To bypass the hook temporarily, use `git push --no-verify`.

## Server

Set `DATABASE_URL` to a MySQL connection string and run:

```bash
dotnet run --project Server/Acderator.Server.csproj
```

The server listens on port `3000` by default. Override it with `PORT`. Other settings live under `Acderator` in `Server/appsettings.json` and can be overridden with environment variables such as `Acderator__MasterDataPath`.

Database migrations are retained in `Server/Migrations`. Master data is loaded from `Server/Data/master.json` at startup. Optional chart JSON files use `Server/Data/charts/{sid}.json`.

Build the .NET projects with:

```bash
dotnet build Acderator.slnx
```

## Docker

Build from the repository root so the shared project is included:

```bash
docker build -f Server/Dockerfile -t acderator-server .
```

## Project Status

- [ ] Update ConfigPopup UI
- [ ] Add Account Linking

## Credits

- [Ched](https://github.com/paralleltree/Ched)
- [MasterMemory](https://github.com/Cysharp/MasterMemory)
- [R3](https://github.com/Cysharp/R3)
- [UIEffect](https://github.com/mob-sakai/UIEffect)
- [UniTask](https://github.com/Cysharp/UniTask)
- [Zenject](https://github.com/modesttree/Zenject)

## License

Acderator is under the [MIT](LICENSE).
