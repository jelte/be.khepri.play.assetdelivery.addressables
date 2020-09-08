using System.ComponentModel;
using Google.Android.AppBundle.Editor;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using PlayAssetPackDeliveryMode = Google.Android.AppBundle.Editor.AssetPackDeliveryMode;

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
                DeliveryMode = (PlayAssetPackDeliveryMode) deliveryMode,
                AssetBundleFilePath = bundle
            };
        }
    }
}