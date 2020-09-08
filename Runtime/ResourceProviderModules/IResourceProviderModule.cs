using System;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets.ResourceModules
{
    public interface IResourceProviderModule
    {
        event Action<IResourceProviderModule, bool, Exception> CompletedEvent;
        
        AssetBundle GetAssetBundle();
        bool TryBeginOperation(ProvideHandle provideHandle);
        void Unload();
    }
}