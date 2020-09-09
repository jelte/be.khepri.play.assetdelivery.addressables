using System;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets.ResourceHandlers
{
    public class LocalAsyncAssetBundleResourceHandler : IAssetBundleResourceHandler
    {
        public event Action<IAssetBundleResourceHandler, bool, Exception> CompletedEvent;
        
        private AssetBundleCreateRequest requestOperation;
        private AssetBundle assetBundle;
        private AssetBundleRequestOptions options;

        AssetBundleRequestOptions IAssetBundleResourceHandler.Options => options;

        public bool TryBeginOperation(ProvideHandle provideHandle)
        {
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (!File.Exists(path))
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
            Debug.LogFormat("[{0}.{1}] path={2}", nameof(LocalAsyncAssetBundleResourceHandler), nameof(BeginOperation), path);
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