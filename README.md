# Play Asset Delivery - Addressables

In short this package provides support for Googles' Play Asset Delivery (https://developer.android.com/guide/app-bundle/asset-delivery) to Addressables.

## Installation

To start you will need to add the package to your project. You can do this via the `Package Manager` or by manually editing the `manifest.json`

### Via Package Manager

1. Press ` + `
2. Select `Add package from git URL...` 
3. url: `https://github.com/jelte/be.khepri.play.assetdelivery.addressables.git#<version>`

### Via manifest.json

```json
   "be.khepri.play.assetdelivery.addressables": "https://github.com/jelte/be.khepri.play.assetdelivery.addressables.git#<version>",
```

##  Configure Groups

`Play Asset Delivery` is not available for Remote asset groups.  

### Content Packing and Loading

- **Bundle Mode**: `Pack Together`
- **Bundle Naming**: `Filename` or `Append Hash to Filename`
- **Asset Provider**: `Assets From Bundles Provider`
- **AssetBundle Provider**: `AssetPack Bundle Provider`

#### Synchronous Addressables

Traditionally, Addressables work Asynchronously. However, this package allows for synchronous loading of assets and asset bundles. 
This relies on the assets to be pre-downloaded, before actually using the asset bundles and assets. 
This is generally done during the initial launch of the game. Once downloaded to the device, loading assets synchronously is smooth.

*note: The process of Pre-downloading and updating asset bundles is not within the scope of this project at the this time.*

To enable synchronous loading, all that is need to be done as set the following settings:

- **Asset Provider**: `Assets From Bundles Provider (Sync)`
- **AssetBundle Provider**: `AssetPack Bundle Provider (Sync)`

The asset bundle provider is a hybrid provider, which allows for both synchronous as asynchronous loading. 
The idea is that after the game has download the assetbundles to the device (either from remote, apk or `Play Asset Delivery`) asynchronously `AssetPackBundleSyncProvider.handleSynchronously` is set to `true`.
After which any attempt loading an asset bundle will be done synchronously. If you chose not to pre-download the assetbundles asynchronously, the your app will be blocked while downloading, which may lead to ANR's.

For more information on Synchronous loading please check out the sample provided by Unity at https://github.com/Unity-Technologies/Addressables-Sample

### Add Schema
The first step is to highlight which Addressable groups should be provided via `Play Asset Delivery` and which should be included in the build.

This is done by adding the `Play Asset Delivery` schema to the Addressable groups. 
Once the schema is added all that is left is to select how the group is to be delivered by settings the `Delivery Mode`:

- **Do Not Package**: Included in the base build.
- **Install Time**: Installed along with the APK.
- **Fast Follow**: Downloaded shortly after.
- **On Demand**: Downloaded at some point late.

*(For more information see: https://developer.android.com/guide/app-bundle/asset-delivery)*

## Building Asset Packs

### Manual build process

1. **Build Addressables**: Before the AAB can be build, the `Addressables` asset bundles need to be build.
2. **Build Asset Pack config**: `Google` > `Create config for Addressables Groups`
3. **Build Android App Bundle**: `Google` > `Build Android App Bundle...`

*Note: Not tested yet*

### Scripted build process

Replace
```csharp
BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
```
with
```csharp
#if UNITY_ANDROID
		if (!AssetPackBuilder.BuildBundleWithAssetPacks(buildPlayerOptions, EditorUserBuildSettings.androidBuildSubtarget, Addressables.BuildPath))
		{
			throw new Exception("BuildScript.Build Failed");
		}
#else
		BuildReport buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
#endif
```

## Questions, Bugs, Feature requests

Please for any of these create a issue on the github repository at https://github.com/jelte/be.khepri.play.assetdelivery.addressables/issues