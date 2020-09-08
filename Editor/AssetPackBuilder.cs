using System.Collections.Generic;
using System.IO;
using System.Linq;
using Google.Android.AppBundle.Editor;
using Google.Android.AppBundle.Editor.Internal;
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
	    public static AssetPackConfig CreateAssetPacks()
		{
			Debug.LogFormat("[{0}.{1}] path={2}", nameof(AssetPackBuilder), nameof(CreateAssetPacks), Addressables.BuildPath);
			AssetPackConfig assetPackConfig = new AssetPackConfig();
			var bundles = GetBundles(Addressables.BuildPath);
			foreach (var bundle in bundles)
			{
				assetPackConfig.AssetPacks.Add(bundle.Name, bundle.CreateAssetPack());
			}
			WriteAssetPackConfig(bundles);
			return assetPackConfig;
		}

	    public static bool BuildBundleWithAssetPacks(BuildPlayerOptions buildPlayerOptions)
	    {
		    if (buildPlayerOptions.target != BuildTarget.Android)
		    {
			    return false;
		    }
		    return AppBundlePublisher.Build(buildPlayerOptions, CreateAssetPacks());
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
				.Where(group => Path.GetFileName(bundle).StartsWith(group.Name))
				.Select(group => group.GetSchema<AssetPackGroupSchema>())
				.FirstOrDefault();
		}

		private static void WriteAssetPackConfig(IEnumerable<AssetPackBundle> packBundles)
		{
			AssetPackBundleConfig config = GetOrCreateConfig();
			config.assetPacks = packBundles.Select(pack => pack.Name).ToArray();
			Debug.LogFormat("[{0}.{1}] bundles={2} path={3}", nameof(AssetPackBuilder), nameof(WriteAssetPackConfig), string.Join(", ", config.assetPacks), AssetPackBundleConfig.PATH);
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