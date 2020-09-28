using System.IO;
using Google.Android.AppBundle.Editor;
using Khepri.AddressableAssets.Editor.Settings.GroupSchemas;
using UDebug = UnityEngine.Debug;

namespace Khepri.AddressableAssets.Editor
{
    internal struct AssetPackBundle
    {
        private const string BUNDLE_SUFFIX = ".bundle";
        private const string CATALOG_BUNDLE = "catalog.bundle";
        
        public string Name { get; }
        public string Bundle { get; }
        public AssetPackGroupSchema Schema { get; }

        public AssetPackDeliveryMode DeliveryMode => Schema.mDeliveryMode;

        public bool IsValid => Schema != null && Bundle.EndsWith(BUNDLE_SUFFIX) && !Bundle.EndsWith(CATALOG_BUNDLE);

        public AssetPackBundle(string bundle, AssetPackGroupSchema schema)
        {
            Name = Path.GetFileNameWithoutExtension(bundle);
            Bundle = bundle;
            Schema = schema;
        }

        public AssetPack CreateAssetPack(TextureCompressionFormat textureCompressionFormat)
        {
            return Schema.CreateAssetPack(Bundle, textureCompressionFormat);
        }

        public void DeleteFile()
        {
            File.Delete(Bundle);
        }
    }
}