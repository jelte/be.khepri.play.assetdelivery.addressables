using System.Linq;
using UnityEngine;

namespace Khepri.AssetDelivery
{
    public class AssetPackBundleConfig : ScriptableObject
    {
        public const string FILENAME = "AssetPacks";
        public const string PATH = "Assets/Resources/AssetPacks.asset";

        public string[] packs;
        
        public bool IsPack(string assetPackName)
        {
            return (packs?.Contains(assetPackName)).GetValueOrDefault(false);
        }
    }
}