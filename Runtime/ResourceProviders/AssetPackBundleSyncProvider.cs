using System;
using System.ComponentModel;
using Khepri.AddressableAssets.ResourceHandlers;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UDebug = UnityEngine.Debug;

namespace Khepri.AddressableAssets.ResourceProviders
{
	[DisplayName("Sync Asset Pack Bundle Provider")]
	public class AssetPackBundleSyncProvider : ResourceProviderBase
    {
	    public static bool handleSynchronously = false;
	    
	    public override void Provide(ProvideHandle providerInterface)
	    {
		    IAssetBundleResourceHandler[] modules = handleSynchronously ? 
			    
			    new IAssetBundleResourceHandler[]
			    {
				    new LocalSyncAssetBundleResourceHandler(),
#if UNITY_ANDROID
				    new AssetPackSyncAssetBundleResourceHandler(),
#endif
				    new WebRequestAssetBundleResourceHandler(true),
				}:
			    new IAssetBundleResourceHandler[]
			    {
				    new LocalAsyncAssetBundleResourceHandler(),
#if UNITY_ANDROID
				    new AssetPackAsyncAssetBundleResourceHandler(),
		            new JarAsyncAssetBundleResourceHandler(),
#endif
				    new WebRequestAssetBundleResourceHandler(false),
			    };
		    new ModularAssetBundleResource(modules).Start(providerInterface);
	    }
		
	    public override Type GetDefaultType(IResourceLocation location)
	    {
		    return typeof(IAssetBundleResource);
	    }

	    public override void Release(IResourceLocation location, object asset)
	    {
		    if (location == null)
		    {
			    throw new ArgumentNullException("location");
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