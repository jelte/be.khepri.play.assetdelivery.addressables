using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Khepri.AddressableAssets.Editor
{
    public class PlayAssetPackBundlesPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 2;
    
        public void OnPreprocessBuild(BuildReport report)
        {
            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }
            foreach (var bundle in AssetPackBuilder.GetBundles(Addressables.PlayerBuildDataPath))
            {
                bundle.DeleteFile();
            };
        }
    }
}