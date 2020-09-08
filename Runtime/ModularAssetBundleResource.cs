using System;
using Khepri.AddressableAssets.BundleResourceModules;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets
{
    public class ModularAssetBundleResource : IAssetBundleResource
    {
        private ProvideHandle provideHandle;
        
        private IBundleResourceModule[] modules;
        private IBundleResourceModule activeModule;

        public ModularAssetBundleResource(IBundleResourceModule[] modules)
        {
            this.modules = modules;
            foreach (var module in modules)
            {
                module.CompletedEvent += OnCompleted;
            }
        }

        private void OnCompleted(IBundleResourceModule module, bool status, Exception exception)
        {
            provideHandle.Complete(this, status, exception);
        }

        internal void Start(ProvideHandle provideHandle)
        {
            activeModule = null;
            this.provideHandle = provideHandle;

            foreach (var module in modules)
            {
                if (module.TryBeginOperation(provideHandle))
                {
                    activeModule = module;
                    return;
                }
            }

            Debug.LogFormat("[{0}.{1}] path={2} Invalid Path", nameof(ModularAssetBundleResource), nameof(Start), provideHandle.Location);
            provideHandle.Complete<ModularAssetBundleResource>(null, false, 
                new Exception($"Invalid path in AssetBundleProvider: '{provideHandle.Location}'."));
        }

        public AssetBundle GetAssetBundle()
        {
            return activeModule?.GetAssetBundle();
        }

        public void Unload()
        {
            activeModule?.Unload();
            activeModule = null;
        }
    }
}