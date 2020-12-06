using System.IO;
using System.Linq;
using Khepri.PlayAssetDelivery.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.AddressableAssets;
using UDebug = UnityEngine.Debug;

public class PlayAssetPackBundlesPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 2;

    public void OnPreprocessBuild(BuildReport report)
    {
        var outputPathExtension = Path.GetExtension(report.summary.outputPath);
        if (report.summary.platform != BuildTarget.Android || !outputPathExtension.Equals(".aab"))
        {
            AssetPackBuilder.ClearConfig();
            return;
        }
        var bundles = AssetPackBuilder.GetBundles(Addressables.PlayerBuildDataPath, SearchOption.AllDirectories);
        if (bundles.Length == 0)
        {
            UDebug.Log($"[{nameof(PlayAssetPackBundlesPreprocessor)}.{nameof(OnPreprocessBuild)}] No bundles removed.");
            return;
        }
        foreach (var bundle in bundles)
        {
            bundle.DeleteFile();
        }
        UDebug.Log($"[{nameof(PlayAssetPackBundlesPreprocessor)}.{nameof(OnPreprocessBuild)}] Removed: \n -{string.Join("\n -", bundles.Select(bundle => bundle.Bundle))}");
    }
}