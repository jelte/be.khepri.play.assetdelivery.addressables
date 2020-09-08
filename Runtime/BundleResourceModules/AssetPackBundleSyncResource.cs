using System;
using System.IO;
using Google.Play.AssetDelivery;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AddressableAssets.BundleResourceModules
{
    public class AssetPackBundleSyncResource : IBundleResourceModule
    {
        public event Action<IBundleResourceModule, bool, Exception> CompletedEvent;
        
        private PlayAssetPackRequest playAssetPackRequest;
        private AssetPackBundleConfig config;        
        private AssetBundle assetBundle;

        public AssetBundle GetAssetBundle()
        {
            return assetBundle;
        }

        public bool TryBeginOperation(ProvideHandle provideHandle)
        {
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (!path.StartsWith(Addressables.RuntimePath) || !path.EndsWith(".bundle"))
            {
                return false;
            }
            string assetPackName = Path.GetFileNameWithoutExtension(path);
            if (!AssetPackBundleConfig.IsAssetPackBundle(assetPackName))
            {
                return false;
            }

            provideHandle.SetProgressCallback(PercentComplete);
            BeginOperation(assetPackName);
            return true;
        }
        
        private void BeginOperation(string assetPackName)
        {
            Debug.LogFormat("[{0}.{1}] assetPackName={2}", nameof(AssetPackBundleSyncResource), nameof(BeginOperation), assetPackName);
            playAssetPackRequest = PlayAssetDelivery.RetrieveAssetPackAsync(assetPackName);
            Exception exception = null;
            if (playAssetPackRequest.IsDone)
            {
                var assetLocation = playAssetPackRequest.GetAssetLocation(assetPackName);
                assetBundle = AssetBundle.LoadFromFile(assetLocation.Path, /* crc= */ 0, assetLocation.Offset);
            }
            else
            {
                exception = new Exception($"Asset Pack was not retrieved Synchronously: '{assetPackName}'.");
            }
            CompletedEvent?.Invoke(this, assetBundle != null, exception);
        }

        private float PercentComplete()
        {
            return 1f;
        }
        
        public void Unload()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }
            if (playAssetPackRequest != null)
            {
                playAssetPackRequest.AttemptCancel();
                playAssetPackRequest = null;
            }
        }
    }
}