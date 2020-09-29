using UnityEngine;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public class JarAsyncAssetBundleResourceHandler : AssetBundleResourceHandlerBase
    {
        protected override bool IsValidPath(string path)
        {
            return path.StartsWith("jar:");
        }
        
        protected override void BeginOperation(string path)
        {
            Debug.LogFormat("[{0}.{1}] path={2}", nameof(JarAsyncAssetBundleResourceHandler), nameof(BeginOperation), path);
            m_RequestOperation = AssetBundle.LoadFromFileAsync(path, Options?.Crc ?? 0);
            if (m_RequestOperation.isDone)
            {
                LocalRequestOperationCompleted(m_RequestOperation);
                return;
            }
            m_RequestOperation.completed += LocalRequestOperationCompleted;
        }

        private void LocalRequestOperationCompleted(AsyncOperation operation)
        {
            CompleteOperation(this, (operation as AssetBundleCreateRequest)?.assetBundle);
        }
    }
}