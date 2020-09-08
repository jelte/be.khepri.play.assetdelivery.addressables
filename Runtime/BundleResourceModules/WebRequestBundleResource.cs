using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.Util;
using UDebug = UnityEngine.Debug;

namespace Khepri.AddressableAssets.BundleResourceModules
{
    public class WebRequestBundleResource : IBundleResourceModule
    {
        public event Action<IBundleResourceModule, bool, Exception> CompletedEvent;

        public bool handleSynchronously = false; 
        
        private ProvideHandle provideHandle;
        private AssetBundleRequestOptions options;
        private WebRequestQueueOperation webRequestQueueOperation;
        private DownloadHandlerAssetBundle downloadHandler;
        private AsyncOperation requestOperation;
        
        private int retries;
        private AssetBundle assetBundle;

        public WebRequestBundleResource(bool handleSynchronously)
        {
            this.handleSynchronously = handleSynchronously;
        }

        public bool TryBeginOperation(ProvideHandle provideHandle)
        {
            retries = 0;
            assetBundle = null;
            downloadHandler = null;
            this.provideHandle = provideHandle;

            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (!ResourceManagerConfig.ShouldPathUseWebRequest(path))
            {
                return false;
            }
            
            options = provideHandle.Location.Data as AssetBundleRequestOptions;
            provideHandle.SetProgressCallback(PercentComplete);
            BeginOperation();
            return true;
        }

        float PercentComplete() { return requestOperation?.progress ?? 0.0f; }

        private void BeginOperation()
        {    
            Debug.LogFormat("[{0}.{1}] location={2}", nameof(WebRequestBundleResource), nameof(BeginOperation), provideHandle.Location);
            var req = CreateWebRequest(provideHandle.Location);
            req.disposeDownloadHandlerOnDispose = false;
            webRequestQueueOperation = WebRequestQueue.QueueRequest(req);
            if (webRequestQueueOperation.IsDone)
            {
                OnQueueOperationCompleted(webRequestQueueOperation.Result);
                return;
            }
            webRequestQueueOperation.OnComplete += OnQueueOperationCompleted;
        }

        private void OnQueueOperationCompleted(UnityWebRequestAsyncOperation asyncOp)
        {
            requestOperation = asyncOp;
            if (!handleSynchronously && !requestOperation.isDone)
            {
                requestOperation.completed += WebRequestOperationCompleted;
                return;
            }

            // Requests are always async, need to wait until done in a sync way.
            // This is mainly for android where the webrequest is used to load bundles from the APK.
            // Download from web should be done during loading, when handleSynchronously should be false. 
            while (!requestOperation.isDone)
            {
            }
            
            WebRequestOperationCompleted(requestOperation);
        }


        UnityWebRequest CreateWebRequest(IResourceLocation loc)
        {
            var url = provideHandle.ResourceManager.TransformInternalId(loc);
            if (options == null)
                return UnityWebRequestAssetBundle.GetAssetBundle(url);

            var webRequest = !string.IsNullOrEmpty(options.Hash) ?
                UnityWebRequestAssetBundle.GetAssetBundle(url, Hash128.Parse(options.Hash), options.Crc) :
                UnityWebRequestAssetBundle.GetAssetBundle(url, options.Crc);

            if (options.Timeout > 0)
                webRequest.timeout = options.Timeout;
            if (options.RedirectLimit > 0)
                webRequest.redirectLimit = options.RedirectLimit;
#if !UNITY_2019_3_OR_NEWER
            webRequest.chunkedTransfer = options.ChunkedTransfer;
#endif
            if (provideHandle.ResourceManager.CertificateHandlerInstance != null)
            {
                webRequest.certificateHandler = provideHandle.ResourceManager.CertificateHandlerInstance;
                webRequest.disposeCertificateHandlerOnDispose = false;
            }
            return webRequest;
        }

        private void WebRequestOperationCompleted(AsyncOperation op)
        {
            UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
            var webReq = remoteReq.webRequest;
            if (string.IsNullOrEmpty(webReq.error))
            {
                downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
                provideHandle.Complete(this, true, null);
            }
            else
            { 
                downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
                downloadHandler.Dispose();
                downloadHandler = null;
                if (retries++ < options.RetryCount)
                {
                    UDebug.LogFormat("Web request {0} failed with error '{1}', retrying ({2}/{3})...", webReq.url, webReq.error, retries, options.RetryCount);
                    BeginOperation();
                }
                else
                {
                    var exception = new Exception(string.Format("RemoteAssetBundleProvider unable to load from url {0}, result='{1}'.", webReq.url, webReq.error));
                    CompletedEvent?.Invoke(null, false, exception);
                }
            }
            webReq.Dispose();
        }

        public AssetBundle GetAssetBundle()
        {
            if (assetBundle == null && downloadHandler != null)
            {
                assetBundle = downloadHandler.assetBundle;
                downloadHandler.Dispose();
                downloadHandler = null;
            }
            return assetBundle;
        }

        public void Unload()
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(true);
                assetBundle = null;
            }
            if (downloadHandler != null)
            {
                downloadHandler.Dispose();
                downloadHandler = null;
            } 
            requestOperation = null;
        }
    }
}