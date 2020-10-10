using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Khepri.AssetDelivery.Operations;
using Khepri.AssetDelivery.ResourceProviders;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UDebug = UnityEngine.Debug;

namespace Khepri.AssetDelivery
{
    public class AddressablesAssetDelivery
    {
        private static AssetPackBundleConfig config;
        private static bool isInitialized;

        public static bool handleSynchronously
        {
            set => AssetPackBundleSyncProvider.handleSynchronously = value;
        }

        private static void TryInitialize()
        {
            if (isInitialized)
            {
                return;
            }
            config = Resources.Load<AssetPackBundleConfig>(AssetPackBundleConfig.FILENAME);
            if (config == null)
            {
                Debug.LogFormat("[{0}.{1}] config not found. (Filename={2})", nameof(AddressablesAssetDelivery), nameof(TryInitialize), AssetPackBundleConfig.FILENAME);
            }
            isInitialized = true;
        }
        
        public static bool IsPack(string name)
        {
            TryInitialize();
            return (config?.IsPack(name)).GetValueOrDefault(false);
        }
        
        public static DownloadSizeAsyncOperation GetDownloadSizeAsync(object key)
        {
            return GetDownloadSizeAsync(new [] {key});
        }

        public static DownloadSizeAsyncOperation GetDownloadSizeAsync(IEnumerable<object> keys)
        {
            IList<string> packs = GetDependencies(keys)
                .Distinct()
                .Select(Addressables.ResourceManager.TransformInternalId)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(IsPack)
                .ToList();

            return new DownloadSizeAsyncOperation(packs);
        }

        private static IList<IResourceLocation> GetDependencies(IEnumerable<object> keys)
        {
            List<IResourceLocation> allLocations = new List<IResourceLocation>();
            foreach (object key in keys)
            {
                IList<IResourceLocation> locations;
                if (key is IList<IResourceLocation>)
                    locations = key as IList<IResourceLocation>;
                else if (key is IResourceLocation)
                    locations = new List<IResourceLocation>(1) { key as IResourceLocation };
                else if (!GetResourceLocations(key, typeof(object), out locations))
                    UDebug.LogWarningFormat("Invalid key: {0}", key);

                foreach (var loc in locations)
                    if(loc.HasDependencies)
                        allLocations.AddRange(loc.Dependencies);
            }

            return allLocations;
        }
        
        private static bool GetResourceLocations(object key, Type type, out IList<IResourceLocation> locations)
        {
            key = EvaluateKey(key);

            locations = null;
            HashSet<IResourceLocation> current = null;
            foreach (var locator in Addressables.ResourceLocators)
            {
                IList<IResourceLocation> locs;
                if (!locator.Locate(key, type, out locs)) 
                    continue;

                if (locations == null)
                {
                    //simple, common case, no allocations
                    locations = locs;
                    continue;
                }
                
                //less common, need to merge...
                if (current == null)
                {
                    current = new HashSet<IResourceLocation>();
                    foreach (var loc in locations)
                        current.Add(loc);
                }

                current.UnionWith(locs);
            }

            if (current == null)
                return locations != null;

            locations = new List<IResourceLocation>(current);
            return true;
        }
        
        private static object EvaluateKey(object obj)
        {
            return (obj as IKeyEvaluator)?.RuntimeKey ?? obj;
        }
    }
}