# Addressables Builder

Unity project for building remote Addressables content for [Acderator](https://github.com/Ierllow/Acderator). Use Unity **6000.3.0f1** and Addressables **3.1.0**, matching the client project.

## Build content

1. Open `Tools/AddressablesBuilder` in Unity 6000.3.0f1.
2. Run **Tools > Sync Content Addresses**. The tool registers `.bytes`, `.spriteatlas`, and `.prefab` files under `Assets/Content` in the `Song` group. Addresses come from paths relative to `Assets/Content`; labels match addresses. A file named `_...` is skipped.
3. Open **Tools > Build Addressables**, select Android or iOS, and build.
4. Upload **all files** from `ServerData/Android` or `ServerData/iOS`, including the catalog, hash, and bundles, to the corresponding path on a static file host. Set the client's `AddressableAssetConfig.asset` `assetServerUrl` to the parent URL.

For example, `Assets/Content/Song/Jacket/jacket.spriteatlas` has address and label `song/jacket`; `Assets/Content/Sounds/Song/song_1.bytes` has `sounds/song/song_1`. The existing `song_se.bytes` has the client key `sounds/song/songse`.

The client load path is `{Acderator.AssetServerUrl}/[BuildTarget]`. The remote catalog uses `PlayerSettings.bundleVersion`, set to `1.0.0` here to match the current Acderator project. Keep it aligned when the client version changes. The hash file must be available with the catalog for `CheckForCatalogUpdates` to detect updates.

The `Server` project provides API routes and does not serve these static files. Use a CDN or separate static file server.

This repository does not contain all assets listed by the client's `AddressableAssetConfig.asset`, including `song/hud`, `song/lane`, and `songselect/banner`. Add those assets under `Assets/Content` as needed and confirm the generated addresses and labels in the Addressables Groups window.

The client player build needs Addressables initialization data. Build the player with compatible Addressables settings and a remote catalog.
