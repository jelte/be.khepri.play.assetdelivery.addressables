using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AssetDelivery.AssetBundles
{
    public struct LoadedAssetBundle
    {
        public readonly string bundleName;
        public readonly string hash;
        public readonly uint crc;
        public readonly AssetBundle assetBundle;

        public LoadedAssetBundle(string bundleName, string hash, uint crc, AssetBundle assetBundle)
        {
            this.bundleName = bundleName;
            this.hash = hash;
            this.crc = crc;
            this.assetBundle = assetBundle;
        }

        public bool Matches(AssetBundleRequestOptions requestOptions)
        {
            return requestOptions.Hash == hash;
        }

        public void Unload()
        {
            if (assetBundle == null)
            {
                return;
            }
            assetBundle.Unload(true);
        }
    }
}