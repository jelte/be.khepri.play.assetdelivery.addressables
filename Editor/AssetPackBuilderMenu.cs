using Google.Android.AppBundle.Editor;
using Google.Android.AppBundle.Editor.Internal;
using UnityEditor;

namespace Khepri.PlayAssetDelivery.Editor
{
    public class AssetPackBuilderMenu
    {
        private const string CreateAction = "Create config for Addressables Groups";
        
        [MenuItem(GoogleEditorMenu.MainMenuName + "/" + CreateAction, false, 101)]
        public static void CreateConfig()
        {
            var assetPackConfig = AssetPackBuilder.CreateAssetPacks(TextureCompressionFormat.Default);
            if (assetPackConfig == null)
            {
                EditorUtility.DisplayDialog(CreateAction, "Unable to create AssetPack config. Make sure Addressables asset bundles have been build!", "Ok");
                return;
            }
        }
    }
}