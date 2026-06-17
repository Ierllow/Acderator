# Acderator

Acderator is a rhythm game made with Unity.
  
## Getting Started

### Requirements

- Unity 6000.3.0f1
- .NET SDK 8.x

You can download the Unity Editor from the [official Download Archive](https://unity.com/releases/editor/archive).

### Installation

Coming soon.

### Building

Open the `App` directory with Unity 6000.3.0f1 and build from Unity Editor.

## Development

Run local checks from the `App` directory.

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
  
## Project Status

- [ ] Update ConfigPopup UI
- [ ] Add Account Linking

## Credits

- [Ched](https://github.com/paralleltree/Ched)
- [MasterMemory](https://github.com/Cysharp/MasterMemory)
- [R3](https://github.com/Cysharp/R3)
- [UIEffect](https://github.com/mob-sakai/UIEffect)
- [uPalette](https://github.com/Haruma-K/uPalette)
- [UniTask](https://github.com/Cysharp/UniTask)
- [Zenject](https://github.com/modesttree/Zenject)

## License

Acderator is under MIT [LICENSE](LICENSE).
