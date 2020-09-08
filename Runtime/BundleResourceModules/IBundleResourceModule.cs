using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets.BundleResourceModules
{
    public interface IBundleResourceModule
    {
        event Action<IBundleResourceModule, bool, Exception> CompletedEvent;
        
        AssetBundle GetAssetBundle();
        bool TryBeginOperation(ProvideHandle provideHandle);
        void Unload();
    }
}