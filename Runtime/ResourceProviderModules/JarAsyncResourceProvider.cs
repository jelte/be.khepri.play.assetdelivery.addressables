using System;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets.ResourceModules
{
    public class JarAsyncResourceProvider : IResourceProviderModule
    {
        private AssetBundleCreateRequest requestOperation;
        private AssetBundle assetBundle;
        private AssetBundleRequestOptions options;

        public event Action<IResourceProviderModule, bool, Exception> CompletedEvent;

        public bool TryBeginOperation(ProvideHandle provideHandle)
        {
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (!path.StartsWith("jar:"))
            {
                return false;
            }
            options = provideHandle.Location.Data as AssetBundleRequestOptions;
            provideHandle.SetProgressCallback(PercentComplete);
            BeginOperation(path);
            return true;
        }
        
        private float PercentComplete() { return requestOperation?.progress ?? 0.0f; }
        
        private void BeginOperation(string path)
        {
            Debug.LogFormat("[{0}.{1}] path={2}", nameof(JarAsyncResourceProvider), nameof(BeginOperation), path);
            requestOperation = AssetBundle.LoadFromFileAsync(path, options?.Crc ?? 0);
            requestOperation.completed += LocalRequestOperationCompleted;
        }

        public AssetBundle GetAssetBundle()
        {
            return assetBundle;
        }

        private void LocalRequestOperationCompleted(AsyncOperation operation)
        {
            assetBundle = (operation as AssetBundleCreateRequest)?.assetBundle;
            CompletedEvent?.Invoke(this, assetBundle != null, null);
        }
        
        public void Unload()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }
            requestOperation = null;
        }

    }
}