using System;
using Khepri.AddressableAssets.ResourceModules;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets
{
    public class ModularAssetBundleResource : IAssetBundleResource
    {
        private ProvideHandle provideHandle;
        
        private IResourceProviderModule[] modules;
        private IResourceProviderModule _activeProviderModule;

        public ModularAssetBundleResource(IResourceProviderModule[] modules)
        {
            this.modules = modules;
            foreach (var module in modules)
            {
                module.CompletedEvent += OnCompleted;
            }
        }

        private void OnCompleted(IResourceProviderModule providerModule, bool status, Exception exception)
        {
            provideHandle.Complete(this, status, exception);
        }

        internal void Start(ProvideHandle provideHandle)
        {
            _activeProviderModule = null;
            this.provideHandle = provideHandle;

            foreach (var module in modules)
            {
                if (module.TryBeginOperation(provideHandle))
                {
                    _activeProviderModule = module;
                    return;
                }
            }

            Debug.LogFormat("[{0}.{1}] path={2} Invalid Path", nameof(ModularAssetBundleResource), nameof(Start), provideHandle.Location);
            provideHandle.Complete<ModularAssetBundleResource>(null, false, 
                new Exception($"Invalid path in AssetBundleProvider: '{provideHandle.Location}'."));
        }

        public AssetBundle GetAssetBundle()
        {
            return _activeProviderModule?.GetAssetBundle();
        }

        public void Unload()
        {
            _activeProviderModule?.Unload();
            _activeProviderModule = null;
        }
    }
}