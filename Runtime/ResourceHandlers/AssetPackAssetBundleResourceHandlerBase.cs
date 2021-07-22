#if UNITY_ANDROID
using System.IO;
using Google.Play.AssetDelivery;
using UnityEngine.AddressableAssets;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public abstract class AssetPackAssetBundleResourceHandlerBase : AssetBundleResourceHandlerBase
    {
        protected PlayAssetPackRequest playAssetPackRequest;

        protected override bool IsValidPath(string path)
        {
            // Only handle local bundles
            if (!path.StartsWith(Addressables.RuntimePath) || !path.EndsWith(".bundle"))
            {
                return false;
            }
            return AddressablesAssetDelivery.IsPack(Path.GetFileNameWithoutExtension(path));
        }

        protected override float PercentComplete()
        {
            return ((playAssetPackRequest?.DownloadProgress ?? .0f) + (m_RequestOperation?.progress ?? .0f)) * .5f;
        }

        protected override void BeginOperation(string path)
        {
            BeginOperationImpl(Path.GetFileNameWithoutExtension(path));
        }

        protected abstract void BeginOperationImpl(string assetPackName);
        
        public override void Unload()
        {
            base.Unload();
            if (playAssetPackRequest != null)
            {
                playAssetPackRequest.AttemptCancel();
                playAssetPackRequest = null;
            }
        }
    }
}
#endif