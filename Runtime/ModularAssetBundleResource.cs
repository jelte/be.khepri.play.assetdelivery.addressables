using System;
using Khepri.AddressableAssets.ResourceHandlers;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets
{
    public class ModularAssetBundleResource : IAssetBundleResource
    {
        private ProvideHandle provideHandle;
        
        private IAssetBundleResourceHandler[] modules;
        private IAssetBundleResourceHandler activeHandler;
        public AssetBundleRequestOptions Options => activeHandler?.Options;

        public ModularAssetBundleResource(IAssetBundleResourceHandler[] modules)
        {
            this.modules = modules;
            foreach (var module in modules)
            {
                module.CompletedEvent += OnCompleted;
            }
        }

        private void OnCompleted(IAssetBundleResourceHandler handler, bool status, Exception exception)
        {
            provideHandle.Complete(this, status, exception);
        }

        internal void Start(ProvideHandle provideHandle)
        {
            activeHandler = null;
            this.provideHandle = provideHandle;

            foreach (var module in modules)
            {
                if (!module.TryBeginOperation(provideHandle)) continue;
                activeHandler = module;
                return;
            }

            Debug.LogFormat("[{0}.{1}] path={2} Invalid Path", nameof(ModularAssetBundleResource), nameof(Start), provideHandle.Location);
            provideHandle.Complete<ModularAssetBundleResource>(null, false, 
                new Exception($"Invalid path in AssetBundleProvider: '{provideHandle.Location}'."));
        }

        public AssetBundle GetAssetBundle()
        {
            return activeHandler?.GetAssetBundle();
        }

        public void Unload()
        {
            activeHandler?.Unload();
            activeHandler = null;
        }
    }
}