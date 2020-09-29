using UnityEngine.Networking;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public class WebRequestSyncAssetBundleResourceHandler : WebRequestAssetBundleResourceHandlerBase
    {
        protected override void BeginOperationImpl(UnityWebRequest request)
        {
            m_RequestOperation = request.SendWebRequest();
            // Requests are always async, need to wait until done in a sync way.
            // This is mainly for android where the webrequest is used to load bundles from the APK.
            // Download from web should be done during loading, when handleSynchronously should be false. 
            while (!m_RequestOperation.isDone)
            {
            }
            
            WebRequestOperationCompleted(m_RequestOperation);    
        }

    }
}