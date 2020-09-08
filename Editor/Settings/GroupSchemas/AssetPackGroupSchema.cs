using System.ComponentModel;
using Google.Android.AppBundle.Editor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Khepri.AddressableAssets.Editor.Settings.GroupSchemas
{
    [DisplayName("Play Asset Delivery")]
    public class AssetPackGroupSchema : AddressableAssetGroupSchema
    {
        [SerializeField]
        AssetPackDeliveryMode deliveryMode;

        public AssetPack CreateAssetPack(string bundle)
        {
            return new AssetPack
            {
                DeliveryMode = deliveryMode,
                AssetBundleFilePath = bundle
            };
        }
    }
}