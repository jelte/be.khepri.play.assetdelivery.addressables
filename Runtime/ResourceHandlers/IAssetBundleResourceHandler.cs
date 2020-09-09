using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets.ResourceHandlers
{
    public interface IAssetBundleResourceHandler
    {
        event Action<IAssetBundleResourceHandler, bool, Exception> CompletedEvent;
        
        AssetBundle GetAssetBundle();
        bool TryBeginOperation(ProvideHandle provideHandle);
        void Unload();

        AssetBundleRequestOptions Options { get; }
    }
}