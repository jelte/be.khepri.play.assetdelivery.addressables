using System;
using System.IO;
using Khepri.AssetDelivery.ResourceHandlers;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AssetDelivery
{
    public class ModularAssetBundleResource : IAssetBundleResource
    {
        private readonly IAssetBundleResourceHandler[] m_Modules;
        private readonly bool m_IsSynchronous;
        
        private ProvideHandle m_ProvideHandle;
        private AssetBundleRequestOptions m_Options; 

        private IAssetBundleResourceHandler m_ActiveHandler;
        private AssetBundle m_AssetBundle;
        private Exception m_OperationException;

        public ModularAssetBundleResource(bool isSynchronous, IAssetBundleResourceHandler[] modules)
        {
            m_IsSynchronous = isSynchronous;
            m_Modules = modules;
        }
        
        internal void Start(ProvideHandle provideHandle)
        {
            m_ActiveHandler = null;
            this.m_ProvideHandle = provideHandle;
            m_Options = provideHandle.Location.Data as AssetBundleRequestOptions;
            foreach (var module in m_Modules)
            {
                if (!module.TryBeginOperation(provideHandle, m_Options, OnCompleted))
                {
                    continue;
                }
                if (!m_IsSynchronous)
                {
                    return;
                }

                Debug.LogFormat("[{0}.{1}] path={2} module={3} success={4}", nameof(ModularAssetBundleResource),
                    nameof(Start), Path.GetFileNameWithoutExtension(provideHandle.Location.InternalId),
                    module.GetType().Name, m_AssetBundle != null);
                bool success = m_AssetBundle != null;
                provideHandle.Complete(this, success, m_OperationException);
                return;
            }

            Debug.LogFormat("[{0}.{1}] path={2} Invalid Path", nameof(ModularAssetBundleResource), nameof(Start), Path.GetFileNameWithoutExtension(provideHandle.Location.InternalId));
            provideHandle.Complete(this, false, new Exception($"Invalid path in AssetBundleProvider: '{provideHandle.Location}'."));
        }

        internal void OnCompleted(IAssetBundleResourceHandler handler, AssetBundle assetBundle, Exception exception)
        {
            Debug.LogFormat("[{0}.{1}] path={2} module={3} success={4}", nameof(ModularAssetBundleResource),
                nameof(OnCompleted), Path.GetFileNameWithoutExtension(m_ProvideHandle.Location.InternalId),
                handler.GetType().Name, assetBundle != null);
            m_ActiveHandler = handler;
            m_AssetBundle = assetBundle;
            m_OperationException = exception;
            if (m_IsSynchronous)
            {
                return;
            }
            m_ProvideHandle.Complete(this, m_AssetBundle != null, m_OperationException);
        }

        public AssetBundle GetAssetBundle()
        {
            return m_AssetBundle;
        }

        public void Unload()
        {
            if (m_AssetBundle != null)
            {
                m_AssetBundle.Unload(true);
                m_AssetBundle = null;
            }
            m_ActiveHandler?.Unload();
            m_ActiveHandler = null;
        }
    }
}