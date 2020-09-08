using System;
using System.ComponentModel;
using Khepri.AddressableAssets.ResourceModules;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UDebug = UnityEngine.Debug;

namespace Khepri.AddressableAssets.Providers
{
	[DisplayName("Async Asset Pack Bundle Provider")]
	public class AssetPackBundleAsyncProvider : ResourceProviderBase
    {
	    public override void Provide(ProvideHandle providerInterface)
	    {
		    new ModularAssetBundleResource(new IResourceProviderModule[]
	            {
	                new LocalAsyncResourceProvider(),
	                new AssetPackAsyncResourceProvider(),
#if UNITY_ANDROID
	                new JarBundleAsyncResource(),
#endif
	                new WebRequestResourceProvider(false),
	            }
			).Start(providerInterface);
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