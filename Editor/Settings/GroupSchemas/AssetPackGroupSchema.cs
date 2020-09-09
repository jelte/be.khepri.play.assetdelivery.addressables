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
        AssetPackDeliveryMode m_DeliveryMode;

        public AssetPackDeliveryMode DeliveryMode => m_DeliveryMode;

        public AssetPack CreateAssetPack(string bundle)
        {
            return new AssetPack
            {
                DeliveryMode = m_DeliveryMode,
                AssetBundleFilePath = bundle
            };
        }
    }
}