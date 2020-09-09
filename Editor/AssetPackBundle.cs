using System.IO;
using Google.Android.AppBundle.Editor;
using Khepri.AddressableAssets.Editor.Settings.GroupSchemas;

namespace Khepri.AddressableAssets.Editor
{
    internal struct AssetPackBundle
    {
        private const string BUNDLE_SUFFIX = ".bundle";
        private const string CATALOG_BUNDLE = "catalog.bundle";
        
        public string Name { get; }
        public string Bundle { get; }
        public AssetPackGroupSchema Schema { get; }

        public AssetPackDeliveryMode DeliveryMode => Schema.DeliveryMode;

        public bool IsValid => Bundle.EndsWith(BUNDLE_SUFFIX) && !Bundle.EndsWith(CATALOG_BUNDLE);

        public AssetPackBundle(string bundle, AssetPackGroupSchema schema)
        {
            Name = Path.GetFileNameWithoutExtension(bundle);
            Bundle = bundle;
            Schema = schema;
        }

        public AssetPack CreateAssetPack()
        {
            return Schema.CreateAssetPack(Bundle);
        }

        public void DeleteFile()
        {
            File.Delete(Bundle);
        }
    }
}