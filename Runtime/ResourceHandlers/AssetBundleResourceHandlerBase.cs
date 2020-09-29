using System;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public abstract class AssetBundleResourceHandlerBase : IAssetBundleResourceHandler
    {
        private Action<IAssetBundleResourceHandler, AssetBundle, Exception> m_CompletedCallback;
        
        protected ProvideHandle m_ProvideHandle;
        protected AsyncOperation m_RequestOperation;
        private AssetBundleRequestOptions m_Options;

        public AssetBundleRequestOptions Options => m_Options;

        public bool TryBeginOperation(ProvideHandle provideHandle, AssetBundleRequestOptions options, Action<IAssetBundleResourceHandler, AssetBundle, Exception> completedCallback)
        {
            string path = provideHandle.ResourceManager.TransformInternalId(provideHandle.Location);
            if (!IsValidPath(path))
            {
                return false;
            }
            m_CompletedCallback = completedCallback;
            m_ProvideHandle = provideHandle;
            m_Options = options;
            provideHandle.SetProgressCallback(PercentComplete);
            BeginOperation(path);
            return true;
        }

        protected abstract bool IsValidPath(string path);

        protected virtual float PercentComplete()
        {
            return m_RequestOperation?.progress ?? .0f;
        }

        protected abstract void BeginOperation(string path);
        
        protected void CompleteOperation(IAssetBundleResourceHandler resourceHandler, string exception)
        {
            Debug.LogFormat("[{0}.{1}] {2} Error={3}", resourceHandler.GetType().Name, nameof(CompleteOperation), Path.GetFileNameWithoutExtension(m_ProvideHandle.Location.InternalId), exception);
            m_CompletedCallback?.Invoke(resourceHandler, null, new Exception(exception));
        }
        
        protected void CompleteOperation(IAssetBundleResourceHandler resourceHandler, AssetBundle assetBundle)
        {
            if (assetBundle == null)
            {
                CompleteOperation(resourceHandler, "Assetbundle failed to load.");
                return;
            }
            Debug.LogFormat("[{0}.{1}] {2} Success={3} hasCallback={4}", resourceHandler.GetType().Name, nameof(CompleteOperation), Path.GetFileNameWithoutExtension(m_ProvideHandle.Location.InternalId), assetBundle != null, m_CompletedCallback != null);
            m_CompletedCallback?.Invoke(resourceHandler, assetBundle, null);
        }

        public virtual void Unload()
        {
            m_RequestOperation = null;
        }
    }
}