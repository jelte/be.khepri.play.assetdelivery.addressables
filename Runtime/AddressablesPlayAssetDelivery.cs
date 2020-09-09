using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Khepri.AddressableAssets.Operations;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UDebug = UnityEngine.Debug;

namespace Khepri.AddressableAssets
{
    public class AddressablesPlayAssetDelivery
    {
        private static AssetPackBundleConfig m_Config;
        private static AssetPackBundleConfig config => m_Config ? m_Config : TryLoad();
        
        private static AssetPackBundleConfig TryLoad()
        {
            return Resources.Load<AssetPackBundleConfig>(AssetPackBundleConfig.PATH) ?? ScriptableObject.CreateInstance<AssetPackBundleConfig>();
        }
        
        public static bool IsPack(string name)
        {
            return config.IsPack(name);
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