using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Android.AppBundle.Editor;
using Google.Android.AppBundle.Editor.AssetPacks;
using Google.Android.AppBundle.Editor.Internal;
using Google.Android.AppBundle.Editor.Internal.BuildTools;
using Khepri.AddressableAssets.Editor.Settings.GroupSchemas;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Khepri.AddressableAssets.Editor
{
    public class AssetPackBuilder
    {
	    /**
	     * Create the Play Asset Delivery configuration.
	     */
	    public static AssetPackConfig CreateAssetPacks(TextureCompressionFormat textureCompressionFormat)
		{
			Debug.LogFormat("[{0}.{1}] path={2}", nameof(AssetPackBuilder), nameof(CreateAssetPacks), Addressables.BuildPath);
			AssetPackConfig assetPackConfig = new AssetPackConfig
			{
				DefaultTextureCompressionFormat = textureCompressionFormat
			};
			if (!Directory.Exists(Addressables.BuildPath))
			{
				return null;
			}
			var bundles = GetBundles(Addressables.BuildPath);
			foreach (var bundle in bundles)
			{
				assetPackConfig.AssetPacks.Add(bundle.Name, bundle.CreateAssetPack(textureCompressionFormat));
			}
			WriteAssetPackConfig(bundles);;
			AssetPackConfigSerializer.SaveConfig(assetPackConfig);
			return assetPackConfig;
		}

	    public static bool BuildBundleWithAssetPacks(BuildPlayerOptions buildPlayerOptions, MobileTextureSubtarget mobileTextureSubtarget)
	    {
		    TextureCompressionFormat textureCompressionFormat = ToTextureCompressionFormat(mobileTextureSubtarget); 
		    return BuildBundleWithAssetPacks(buildPlayerOptions, textureCompressionFormat);
	    }

	    public static TextureCompressionFormat ToTextureCompressionFormat(MobileTextureSubtarget mobileTextureSubtarget)
	    {
		    switch (mobileTextureSubtarget)
		    {
			    case MobileTextureSubtarget.Generic:
				    return TextureCompressionFormat.Default;
			    case MobileTextureSubtarget.DXT:
				    return TextureCompressionFormat.Dxt1;
			    case MobileTextureSubtarget.PVRTC:
				    return TextureCompressionFormat.Pvrtc;
			    case MobileTextureSubtarget.ETC:
				    return TextureCompressionFormat.Etc1;
			    case MobileTextureSubtarget.ETC2:
				    return TextureCompressionFormat.Etc2;
			    case MobileTextureSubtarget.ASTC:
				    return TextureCompressionFormat.Astc;
			    default:
				    throw new ArgumentOutOfRangeException(nameof(mobileTextureSubtarget), mobileTextureSubtarget, null);
		    }
	    }
	    
	    public static bool BuildBundleWithAssetPacks(BuildPlayerOptions buildPlayerOptions, TextureCompressionFormat textureCompressionFormat)
	    {
		    if (buildPlayerOptions.target != BuildTarget.Android)
		    {
			    return false;
		    }
		    return AppBundlePublisher.Build(buildPlayerOptions, CreateAssetPacks(textureCompressionFormat));
	    }

	    internal static AssetPackBundle[] GetBundles(string path)
	    {
		    return Directory.GetFiles(path)
			    .Select(file => new AssetPackBundle(file, GetAssetPackGroupSchema(file)))
			    .Where(pack => pack.IsValid)
			    .ToArray();
	    }

	    /**
	     * Determine the AssetPackGroupSchema associated with this asset bundle.
	     * 
	     * @param string path of the bundle
	     */
		private static AssetPackGroupSchema GetAssetPackGroupSchema(string bundle)
		{
			return AddressableAssetSettingsDefaultObject.Settings.groups
				.Where(group => group.HasSchema<AssetPackGroupSchema>())
				.Where(group => Path.GetFileName(bundle).StartsWith(group.Name.ToLower()))
				.Select(group => group.GetSchema<AssetPackGroupSchema>())
				.FirstOrDefault();
		}

		private static void WriteAssetPackConfig(IEnumerable<AssetPackBundle> packBundles)
		{
			AssetPackBundleConfig config = GetOrCreateConfig();
			config.packs = packBundles.Select(pack => pack.Name).ToArray();
			Debug.LogFormat("[{0}.{1}] path={2}", nameof(AssetPackBuilder), nameof(WriteAssetPackConfig), AssetPackBundleConfig.PATH);
			EditorUtility.SetDirty(config);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private static AssetPackBundleConfig GetOrCreateConfig()
		{
			AssetPackBundleConfig config = AssetDatabase.LoadAssetAtPath<AssetPackBundleConfig>(AssetPackBundleConfig.PATH);
			if (config == null)
			{
				config = ScriptableObject.CreateInstance<AssetPackBundleConfig>();
				AssetDatabase.CreateAsset(config, AssetPackBundleConfig.PATH);
			}
			return config;
		}
    }
}