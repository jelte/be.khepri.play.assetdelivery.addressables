using System.IO;
using UnityEngine;

namespace Khepri.AssetDelivery.ResourceHandlers
{
    public class LocalSyncAssetBundleResourceHandler : AssetBundleResourceHandlerBase
    {
        protected override bool IsValidPath(string path)
        {
            return File.Exists(path);
        }

        protected override float PercentComplete()
        {
            return 1f;
        }
        
        protected override void BeginOperation(string path)
        {
            Debug.LogFormat("[{0}.{1}] path={2}", nameof(LocalSyncAssetBundleResourceHandler), nameof(BeginOperation), path);
            AssetBundle assetBundle = AssetBundle.LoadFromFile(path, Options?.Crc ?? 0);
            CompleteOperation(this, assetBundle);
        }
    }
}