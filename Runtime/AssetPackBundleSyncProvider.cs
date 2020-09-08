using System;
using System.ComponentModel;
using Khepri.AddressableAssets.BundleResourceModules;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UDebug = UnityEngine.Debug;

namespace Khepri.AddressableAssets
{
	[DisplayName("Async Asset Pack Bundle Provider")]
    public class AssetPackBundleSyncProvider : ResourceProviderBase
    {
	    public static bool handleSynchronously = false;
	    
	    public override void Provide(ProvideHandle providerInterface)
	    {
		    IBundleResourceModule[] modules = handleSynchronously ? 
			    
			    new IBundleResourceModule[]
			    {
				    new LocalBundleSyncResource(),
				    new AssetPackBundleSyncResource(),
				    new WebRequestBundleResource(true),
				}:
			    new IBundleResourceModule[]
			    {
				    new LocalBundleAsyncResource(),
				    new AssetPackBundleAsyncResource(),
		#if UNITY_ANDROID
		                new JarBundleAsyncResource(),
		#endif
				    new WebRequestBundleResource(false),
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