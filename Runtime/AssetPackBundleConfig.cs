using System.Linq;
using UnityEngine;

namespace Khepri.AddressableAssets
{
    public class AssetPackBundleConfig : ScriptableObject
    {
        public const string PATH = "Assets/Resources/AssetPacks.asset";
        
        private static AssetPackBundleConfig instance;
        private static AssetPackBundleConfig Instance => instance ?? TryLoad();

        private static AssetPackBundleConfig TryLoad()
        {
            AssetPackBundleConfig config = Resources.Load<AssetPackBundleConfig>(PATH);
            if (config == null)
            {
                config = new AssetPackBundleConfig();
            }
            return config;
        }

        public string[] bundles;

        public static bool IsAssetPackBundle(string assetPackName)
        {
            return (Instance?.bundles?.Any(assetPackName.Equals)).GetValueOrDefault(false);
        }
    }
}