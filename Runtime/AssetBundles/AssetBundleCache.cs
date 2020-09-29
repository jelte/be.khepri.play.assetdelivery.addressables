using System.Collections.Generic;
using System.Linq;
using Khepri.AssetDelivery.AssetBundles;
using Khepri.AssetDelivery.Utils;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AssetDelivery.AssetBundles
{
    public class AssetBundleCache
    {
        private static Dictionary<string, LoadedAssetBundle> assetBundles = new Dictionary<string, LoadedAssetBundle>();
        
		public static void UnloadBundles(IList<IAssetBundleResource> resources)
		{
			IEnumerable<AssetBundleRequestOptions> bundleNamesToUnload = resources.Select(resource => GetOptions(resource)).Where(options => options != null);
			foreach (AssetBundleRequestOptions bundleRequestOptions in bundleNamesToUnload)
			{
				if (!assetBundles.TryGetValue(bundleRequestOptions.BundleName, out LoadedAssetBundle assetBundle) || assetBundle.Matches(bundleRequestOptions))
				{
					continue;
				}
				assetBundle.Unload();
				assetBundles.Remove(bundleRequestOptions.BundleName);
			}
		}

		public static AssetBundle[] LoadBundles(IList<IAssetBundleResource> resources)
		{
			foreach (IAssetBundleResource resource in resources)
			{
				TryLoadBundle(resource);
			}
			return assetBundles.Values.Select(loadedAssetbundle => loadedAssetbundle.assetBundle).ToArray();
		}

		public static AssetBundle TryLoadBundle(IAssetBundleResource resource)
		{
			AssetBundleRequestOptions requestOptions = GetOptions(resource);
			if (requestOptions == null)
			{
				Debug.LogFormat("[{0}.{1}] Missing RequestOptions", nameof(AssetBundleCache), nameof(TryLoadBundle));
				return null;
			}
			if (assetBundles.TryGetValue(requestOptions.BundleName, out LoadedAssetBundle loadedAssetBundle))
			{
				if (loadedAssetBundle.Matches(requestOptions) && loadedAssetBundle.assetBundle != null)
				{
					Debug.LogFormat("[{0}.{1}] assetBundle={2} already loaded", nameof(AssetBundleCache), nameof(TryLoadBundle), requestOptions.BundleName);
					return loadedAssetBundle.assetBundle;
				}
				Debug.LogFormat("[{0}.{1}] assetBundle={2} unloading", nameof(AssetBundleCache), nameof(TryLoadBundle), requestOptions.BundleName);
				loadedAssetBundle.Unload();
			}
			Debug.LogFormat("[{0}.{1}] assetBundle={2} loading", nameof(AssetBundleCache), nameof(TryLoadBundle), requestOptions.BundleName);
			AssetBundle assetBundle = resource.GetAssetBundle();
			loadedAssetBundle = new LoadedAssetBundle(requestOptions.BundleName, requestOptions.Hash, requestOptions.Crc, assetBundle);
			assetBundles[requestOptions.BundleName] = loadedAssetBundle;
			Debug.LogFormat("[{0}.{1}] assetBundle={2} loaded={3}", nameof(AssetBundleCache), nameof(TryLoadBundle), requestOptions.BundleName, assetBundle != null);
			return assetBundle;
		}

		private static AssetBundleRequestOptions GetOptions(IAssetBundleResource resource)
		{
			if (resource == null)
			{
				return null;
			}
			return ReflectionUtils.GetPrivateFieldValue<AssetBundleRequestOptions>(resource, "m_Options");
		}
    }
}