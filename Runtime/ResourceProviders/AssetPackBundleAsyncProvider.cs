using System;
using System.ComponentModel;
using Khepri.AssetDelivery.ResourceHandlers;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UDebug = UnityEngine.Debug;

namespace Khepri.AssetDelivery.ResourceProviders
{
	[DisplayName("Asset Pack Bundle Provider")]
	public class AssetPackBundleAsyncProvider : ResourceProviderBase
	{
		public override void Provide(ProvideHandle provideHandle)
	    {
		    new ModularAssetBundleResource(false, new IAssetBundleResourceHandler[]
		    {
			    new LocalAsyncAssetBundleResourceHandler(),
#if UNITY_ANDROID
			    new AssetPackAsyncAssetBundleResourceHandler(),
			    new JarAsyncAssetBundleResourceHandler(),
#endif
			    new WebRequestAsyncAssetBundleResourceHandler(),
		    }).Start(provideHandle);
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