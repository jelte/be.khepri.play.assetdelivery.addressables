/*******
 * Source: https://github.com/Unity-Technologies/Addressables-Sample/blob/master/Advanced/Sync%20Addressables/Assets/SyncAddressables/SyncBundledAssetProvider.cs
 ******/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Khepri.AssetDelivery.AssetBundles;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Khepri.AssetDelivery.ResourceProviders
{
	[DisplayName("Assets From Bundles Provider (Sync)")]
	public class SyncBundledAssetProvider : BundledAssetProvider
	{
		private class InternalOp
		{
			private static AssetBundle LoadBundleFromDependecies(IList<object> results)
			{
				return results?.OfType<IAssetBundleResource>()
					.Select(AssetBundleCache.TryLoadBundle)
					.FirstOrDefault();
			}
			
			public void Start(ProvideHandle provideHandle)
			{
				Type t = provideHandle.Type;
				List<object> deps = new List<object>();
				provideHandle.GetDependencies(deps);
				Debug.LogFormat("[{0}.{1}] path={2} deps={3}", nameof(SyncBundledAssetProvider), nameof(Start), provideHandle.Location.InternalId, deps.Count);
				AssetBundle bundle = LoadBundleFromDependecies(deps);
				Debug.LogFormat("[{0}.{1}] path={2} deps={3} hasBundle={4}", nameof(SyncBundledAssetProvider), nameof(Start), provideHandle.Location.InternalId, deps.Count, bundle != null);
				if (bundle == null)
				{
					provideHandle.Complete<AssetBundle>(null, false, new Exception("Unable to load dependent bundle from location " + provideHandle.Location));
					return;
				}

				object result = null;
				if (t.IsArray)
					result = bundle.LoadAssetWithSubAssets(provideHandle.Location.InternalId, t.GetElementType());
				else if (t.IsGenericType && typeof(IList<>) == t.GetGenericTypeDefinition())
					result = bundle.LoadAssetWithSubAssets(provideHandle.Location.InternalId, t.GetElementType()).ToList();
				else
					result = bundle.LoadAsset(provideHandle.Location.InternalId, t);
				
				provideHandle.Complete(result, result != null, null);
			}
		}
	
		public override void Provide(ProvideHandle provideHandle)
		{
			Debug.LogFormat("[{0}.{1}] path={2}", nameof(SyncBundledAssetProvider), nameof(Provide), provideHandle.Location.InternalId);
			new InternalOp().Start(provideHandle);   
		}
	}
}
