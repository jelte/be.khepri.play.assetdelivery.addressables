using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Khepri.AddressableAssets
{
    public class AssetPackBundleConfig : ScriptableObject
    {
        public const string PATH = "Assets/Resources/AssetPacks.asset";
        
        private static AssetPackBundleConfig instance;
        public static AssetPackBundleConfig Instance => instance ?? TryLoad();
        public IList<string> launchPacks => installTimePacks.Concat(fastFollowPacks).ToList();

        private static AssetPackBundleConfig TryLoad()
        {
            AssetPackBundleConfig config = Resources.Load<AssetPackBundleConfig>(PATH);
            if (config == null)
            {
                config = new AssetPackBundleConfig();
            }
            return config;
        }

        public string[] installTimePacks;
        public string[] fastFollowPacks;
        public string[] onDemandPacks;

        public static bool IsPack(string name)
        {
            return IsInstallTime(name) || IsFastFollow(name) || IsOnDemand(name);
        }
        
        public static bool IsInstallTime(string assetPackName)
        {
            return (Instance?.installTimePacks?.Any(assetPackName.Equals)).GetValueOrDefault(false);
        }

        public static bool IsFastFollow(string assetPackName)
        {
            return (Instance?.fastFollowPacks?.Any(assetPackName.Equals)).GetValueOrDefault(false);
        }

        public static bool IsOnDemand(string assetPackName)
        {
            return (Instance?.onDemandPacks?.Any(assetPackName.Equals)).GetValueOrDefault(false);
        }
    }
}