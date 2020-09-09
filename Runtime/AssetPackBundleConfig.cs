using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Khepri.AddressableAssets
{
    public class AssetPackBundleConfig : ScriptableObject
    {
        public const string PATH = "Assets/Resources/AssetPacks.asset";

        public string[] packs;
        
        public bool IsPack(string assetPackName)
        {
            return packs.Contains(assetPackName);
        }
    }
}