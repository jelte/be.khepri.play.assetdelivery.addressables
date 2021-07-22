#if UNITY_ANDROID
using Google.Play.AssetDelivery;
using UnityEngine;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public class AssetPackSyncAssetBundleResourceHandler : AssetPackAssetBundleResourceHandlerBase
    {
        protected override void BeginOperationImpl(string assetPackName)
        {
            Debug.LogFormat("[{0}.{1}] assetPackName={2}", nameof(AssetPackSyncAssetBundleResourceHandler), nameof(BeginOperation), assetPackName);
            playAssetPackRequest = PlayAssetDelivery.RetrieveAssetPackAsync(assetPackName);
            if (!playAssetPackRequest.IsDone)
            {
                CompleteOperation(this, $"Asset Pack was not retrieved Synchronously: '{assetPackName}'.");
                return;
            }
            var assetLocation = playAssetPackRequest.GetAssetLocation(assetPackName);
            var assetBundle = AssetBundle.LoadFromFile(assetLocation.Path, 0, assetLocation.Offset);
            CompleteOperation(this, assetBundle);
        }
    }
}
#endif