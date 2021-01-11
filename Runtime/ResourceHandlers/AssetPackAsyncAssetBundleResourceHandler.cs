#if UNITY_ANDROID
using Google.Play.AssetDelivery;
using UnityEngine;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public class AssetPackAsyncAssetBundleResourceHandler : AssetPackAssetBundleResourceHandlerBase
    {
        protected override void BeginOperationImpl(string assetPackName)
        {
            Debug.LogFormat("[{0}.{1}] assetPackName={2}", nameof(AssetPackAsyncAssetBundleResourceHandler), nameof(BeginOperation), assetPackName);
            playAssetPackRequest = PlayAssetDelivery.RetrieveAssetPackAsync(assetPackName);
            if (playAssetPackRequest.IsDone)
            {
                OnPlayAssetPackRequestCompleted(assetPackName, playAssetPackRequest);
                return;
            }
            playAssetPackRequest.Completed += request => OnPlayAssetPackRequestCompleted(assetPackName, request);
        }

        private void OnPlayAssetPackRequestCompleted(string assetPackName, PlayAssetPackRequest request)
        {
            if (request.Error != AssetDeliveryErrorCode.NoError)
            {
                CompleteOperation(this, $"Error downloading error pack: {request.Error}");
                return;
            }
            if (request.Status != AssetDeliveryStatus.Available)
            {
                CompleteOperation(this, $"Error downloading status: {request.Status}");
                return;
            }
            var assetLocation = request.GetAssetLocation(assetPackName);
            m_RequestOperation = AssetBundle.LoadFromFileAsync(assetLocation.Path, /* crc= */ 0, assetLocation.Offset);
            if (m_RequestOperation.isDone)
            {
                LocalMRequestOperationCompleted(m_RequestOperation);
                return;
            }
            m_RequestOperation.completed += LocalMRequestOperationCompleted;
        }
        
        private void LocalMRequestOperationCompleted(AsyncOperation op)
        {
            CompleteOperation(this, (op as AssetBundleCreateRequest)?.assetBundle);
        }
    }
}
#endif