using System.IO;
using System.Linq;
using Khepri.PlayAssetDelivery.Editor;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.AddressableAssets;
using UDebug = UnityEngine.Debug;

#if UNITY_2021_2_OR_NEWER
public class PlayAssetPackBundlesPreprocessor : BuildPlayerProcessor
{
    public override int callbackOrder => -1;
    
    public override void PrepareForBuild(BuildPlayerContext buildPlayerContext)
    {
        AddressablesPlayerBuildProcessor.BuildAddressablesOverride = settings =>
        {
            AddressableAssetSettings.BuildPlayerContent(out var result);
            PostBuild(buildPlayerContext);
            return result;
        };
    }

    private void PostBuild(BuildPlayerContext buildPlayerContext)
    {
        var outputPathExtension = Path.GetExtension(buildPlayerContext.BuildPlayerOptions.locationPathName);
        if (buildPlayerContext.BuildPlayerOptions.target != BuildTarget.Android || !outputPathExtension.Equals(".aab"))
        {
            AssetPackBuilder.ClearConfig();
            return;
        }
        var bundles = AssetPackBuilder.GetBundles(Addressables.BuildPath, SearchOption.AllDirectories);
        if (bundles.Length == 0)
        {
            UDebug.Log($"[{nameof(PlayAssetPackBundlesPreprocessor)}.{nameof(PrepareForBuild)}] No bundles removed.");
            return;
        }
        foreach (var bundle in bundles)
        {
            bundle.DeleteFile();
        }

        UDebug.Log($"[{nameof(PlayAssetPackBundlesPreprocessor)}.{nameof(PrepareForBuild)}] Removed: \n -{string.Join("\n -", bundles.Select(bundle => bundle.Bundle))}");
    }
}
#else
public class PlayAssetPackBundlesPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        var outputPathExtension = Path.GetExtension(report.summary.outputPath);
        if (report.summary.platform != BuildTarget.Android || !outputPathExtension.Equals(".aab"))
        {
            AssetPackBuilder.ClearConfig();
            return;
        }
        var bundles = AssetPackBuilder.GetBundles(Addressables.BuildPath, SearchOption.AllDirectories);
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
#endif