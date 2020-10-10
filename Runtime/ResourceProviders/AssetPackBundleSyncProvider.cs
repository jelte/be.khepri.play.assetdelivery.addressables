using System;
using System.ComponentModel;
using Khepri.AssetDelivery.ResourceHandlers;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UDebug = UnityEngine.Debug;

namespace Khepri.AssetDelivery.ResourceProviders
{
	[DisplayName("Asset Pack Bundle Provider (Sync)")]
	public class AssetPackBundleSyncProvider : ResourceProviderBase
    {
	    public static bool handleSynchronously = false;

	    public override void Provide(ProvideHandle provideHandle)
	    {
		    Debug.LogFormat("[{0}.{1}] Type={2} Location={3} handleSynchronously={4}", nameof(AssetPackBundleSyncProvider), nameof(Provide), provideHandle.Type, provideHandle.Location, handleSynchronously);
		    new ModularAssetBundleResource(handleSynchronously, GetModules()).Start(provideHandle);
	    }

	    private IAssetBundleResourceHandler[] GetModules()
	    {
		    if (handleSynchronously)
		    {
			    return new IAssetBundleResourceHandler[]
			    {
				    new LocalSyncAssetBundleResourceHandler(),
#if UNITY_ANDROID
				    new AssetPackSyncAssetBundleResourceHandler(),
#endif
				    new WebRequestSyncAssetBundleResourceHandler(),
			    };
		    }
		    return new IAssetBundleResourceHandler[]
		    {
			    new LocalAsyncAssetBundleResourceHandler(),
#if UNITY_ANDROID
			    new AssetPackAsyncAssetBundleResourceHandler(),
			    new JarAsyncAssetBundleResourceHandler(),
#endif
			    new WebRequestAsyncAssetBundleResourceHandler(),
		    };
	    }
		
	    public override Type GetDefaultType(IResourceLocation location)
	    {
		    return typeof(IAssetBundleResource);
	    }

	    public override void Release(IResourceLocation location, object asset)
	    {
		    if (location == null)
		    {
			    throw new ArgumentNullException(nameof(location));
		    }

		    if (asset == null)
		    {
			    UDebug.LogWarningFormat("Releasing null asset bundle from location {0}.  This is an indication that the bundle failed to load.", location);
			    return;
		    }

		    if (asset is ModularAssetBundleResource bundle)
		    {
			    bundle.Unload();
		    }
	    }
    }
}