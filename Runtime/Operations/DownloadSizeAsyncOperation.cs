using System.Collections.Generic;
using System.Linq;
using Google.Play.AssetDelivery;
using Google.Play.Common;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Khepri.AddressableAssets.Operations
{
 public class DownloadSizeAsyncOperation : AsyncOperationBase<long>
    {
        private IList<string> assetPacks;
        private List<PlayAsyncOperation<long, AssetDeliveryErrorCode>> assetPackSizeOperations;
        private bool completed = false;

        public DownloadSizeAsyncOperation(IList<string> assetPacks)
        {
            this.assetPacks = assetPacks;
        }

        protected override void Execute()
        {
            assetPackSizeOperations = new List<PlayAsyncOperation<long, AssetDeliveryErrorCode>>();
            foreach (string assetPack in assetPacks)
            {
                // check if the asset pack was already downloaded either during install or previous launch.
                if (PlayAssetDelivery.IsDownloaded(assetPack))
                {
                    continue;
                }
                PlayAsyncOperation<long, AssetDeliveryErrorCode> sizeOperation = PlayAssetDelivery.GetDownloadSize(assetPack);
                assetPackSizeOperations.Add(sizeOperation);
                if (sizeOperation.IsDone)
                {
                    OnGetPackDownloadSize(sizeOperation);
                }
                else
                {
                    sizeOperation.Completed += OnGetPackDownloadSize;   
                }
            }
        }

        private void OnGetPackDownloadSize(PlayAsyncOperation<long, AssetDeliveryErrorCode> playAsyncOperation)
        {
            // Wait until all size operations are completed.
            if (assetPackSizeOperations.Any(operation => !operation.IsDone))
                return;

            // Already completed.
            if (completed)
                return;
            completed = true;
            
            // At least one operation failed.
            if (assetPackSizeOperations.Any(operation => !operation.IsSuccessful))
            {
                AssetDeliveryErrorCode errorCode = assetPackSizeOperations
                    .Where(operation => !operation.IsSuccessful)
                    .Select(op => op.Error)
                    .FirstOrDefault();
                Complete(0L, false, $"Failed to retrieve pending assetPack size {errorCode}");
                return;
            }

            long size = assetPackSizeOperations.Sum(operation => operation.GetResult());
            Complete(size, true, null);
        }
    }
}