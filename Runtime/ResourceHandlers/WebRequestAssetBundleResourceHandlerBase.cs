using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.Util;
using UDebug = UnityEngine.Debug;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public abstract class WebRequestAssetBundleResourceHandlerBase : AssetBundleResourceHandlerBase
    {
        private int m_retries;

        protected override bool IsValidPath(string path)
        {
            return ResourceManagerConfig.ShouldPathUseWebRequest(path);
        }

        protected override void BeginOperation(string path)
        {
            Debug.LogFormat("[{0}.{1}] location={2}", nameof(WebRequestAssetBundleResourceHandlerBase), nameof(BeginOperation), m_ProvideHandle.Location);
            var req = CreateWebRequest(path);
            req.disposeDownloadHandlerOnDispose = false;
            BeginOperationImpl(req);
        }

        protected abstract void BeginOperationImpl(UnityWebRequest request);
 
        UnityWebRequest CreateWebRequest(string url)
        {
            if (Options == null)
                return UnityWebRequestAssetBundle.GetAssetBundle(url);

            var webRequest = !string.IsNullOrEmpty(Options.Hash) ?
                UnityWebRequestAssetBundle.GetAssetBundle(url, Hash128.Parse(Options.Hash), Options.Crc) :
                UnityWebRequestAssetBundle.GetAssetBundle(url, Options.Crc);

            if (Options.Timeout > 0)
                webRequest.timeout = Options.Timeout;
            if (Options.RedirectLimit > 0)
                webRequest.redirectLimit = Options.RedirectLimit;
#if !UNITY_2019_3_OR_NEWER
            webRequest.chunkedTransfer = Options.ChunkedTransfer;
#endif
            if (m_ProvideHandle.ResourceManager.CertificateHandlerInstance != null)
            {
                webRequest.certificateHandler = m_ProvideHandle.ResourceManager.CertificateHandlerInstance;
                webRequest.disposeCertificateHandlerOnDispose = false;
            }
            return webRequest;
        }

        protected void WebRequestOperationCompleted(AsyncOperation op)
        {
            UnityWebRequestAsyncOperation remoteReq = op as UnityWebRequestAsyncOperation;
            var webReq = remoteReq.webRequest;
            var downloadHandler = webReq.downloadHandler as DownloadHandlerAssetBundle;
            if (downloadHandler == null || webReq.isHttpError || !webReq.isNetworkError || string.IsNullOrEmpty(webReq.error))
            {
                downloadHandler?.Dispose();
                if (m_retries++ < Options.RetryCount)
                {
                    UDebug.LogFormat("Web request {0} failed with error '{1}', retrying ({2}/{3})...", webReq.url, webReq.error, m_retries, Options.RetryCount);
                    BeginOperation(webReq.url);
                }
                else
                {
                    CompleteOperation(this, $"RemoteAssetBundleProvider unable to load from url {webReq.url}, result='{webReq.error}'.");
                }
                webReq.Dispose();
                return;
            }
            CompleteOperation(this, downloadHandler.assetBundle);
            downloadHandler.Dispose();
            webReq.Dispose();
        }
    }
}