using Khepri.AddressableAssets.Operations;

namespace Khepri.AddressableAssets
{
    public class AddressablesPlayAssetDelivery
    {
        /**
         * Retrieve the download size for all pending asset packs
         */
        public static DownloadSizeAsyncOperation GetDownloadSizeAsync()
        {
            return new DownloadSizeAsyncOperation(AssetPackBundleConfig.Instance.launchPacks);
        }
        
        public static bool IsPack(string name)
        {
            return AssetPackBundleConfig.IsPack(name);
        }
    }
}