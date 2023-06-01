using UnityEngine.Networking;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public class WebRequestAsyncAssetBundleResourceHandler : WebRequestAssetBundleResourceHandlerBase
    {
        private WebRequestQueueOperation m_WebRequestQueueOperation;
        
        protected override void BeginOperationImpl(UnityWebRequest request)
        {
            m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(request);
            if (m_WebRequestQueueOperation.IsDone)
            {
                OnQueueOperationCompleted(m_WebRequestQueueOperation.Result);
                return;
            }
            m_WebRequestQueueOperation.OnComplete += OnQueueOperationCompleted;
        }

        private void OnQueueOperationCompleted(UnityWebRequestAsyncOperation asyncOp)
        {
            SetWebRequest(asyncOp.webRequest);
            m_RequestOperation = asyncOp;
            if (m_RequestOperation.isDone)
            {
                WebRequestOperationCompleted(m_RequestOperation);
                return;
            }
            m_RequestOperation.completed += WebRequestOperationCompleted;
        }
    }
}