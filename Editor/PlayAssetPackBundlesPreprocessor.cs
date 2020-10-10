using System.IO;
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
        if (report.summary.platform != BuildTarget.Android)
        {
            return;
        }
        if (!Path.GetExtension(report.summary.outputPath).Equals("aab"))
        {
            return;
        }
        UDebug.Log($"[{nameof(PlayAssetPackBundlesPreprocessor)}.{nameof(OnPreprocessBuild)}]");
        foreach (var bundle in AssetPackBuilder.GetBundles(Addressables.PlayerBuildDataPath))
        {
            bundle.DeleteFile();
        };
    }
}