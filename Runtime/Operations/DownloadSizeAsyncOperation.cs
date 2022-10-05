#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using Google.Play.AssetDelivery;
using Google.Play.Common;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Khepri.AssetDelivery.Operations
{
    public class DownloadSizeAsyncOperation : CustomYieldInstruction
    {
        private List<PlayAsyncOperation<long, AssetDeliveryErrorCode>> assetPackSizeOperations;
        private bool completed = false;

        /// <summary>
        /// Whether or not the operation has finished.
        /// </summary>
        public bool IsDone { get; private set; }
        public override bool keepWaiting => !IsDone;
        
        public long Result { get; private set; }
        
        public AsyncOperationStatus Status { get; private set; }
        public Exception OperationException { get; private set; }

        public DownloadSizeAsyncOperation(IList<string> assetPacks)
        {
            assetPackSizeOperations = new List<PlayAsyncOperation<long, AssetDeliveryErrorCode>>();
            foreach (string assetPack in assetPacks)
            {
                // check if the asset pack was already downloaded either during install or previous launch.
                if (PlayAssetDelivery.IsDownloaded(assetPack))
                {
                    Debug.LogFormat("[{0}] Assetpack={1} Already downloaded", nameof(DownloadSizeAsyncOperation), assetPack);
                    continue;
                }
                PlayAsyncOperation<long, AssetDeliveryErrorCode> sizeOperation = PlayAssetDelivery.GetDownloadSize(assetPack);
                assetPackSizeOperations.Add(sizeOperation);
                if (sizeOperation.IsDone)
                {
                    OnGetPackDownloadSize(sizeOperation);
                    continue;
                }
                sizeOperation.Completed += OnGetPackDownloadSize;
            }

            // No Asset packs needed to be updated.
            if (assetPackSizeOperations.Count == 0)
            {
                Complete(AsyncOperationStatus.Succeeded);
            }
        }

        private void OnGetPackDownloadSize(PlayAsyncOperation<long, AssetDeliveryErrorCode> playAsyncOperation)
        {
            if (!playAsyncOperation.IsSuccessful)
                Debug.LogFormat("[{0}.{1}] Error={2}", nameof(DownloadSizeAsyncOperation), nameof(OnGetPackDownloadSize), playAsyncOperation.Error);
            
            // Wait until all size operations are completed.
            if (assetPackSizeOperations.Any(operation => !operation.IsDone))
                return;

            // Already completed.
            if (completed)
                return;
            completed = true;

            // At least one operation failed.
            if (!assetPackSizeOperations.All(operation => operation.IsSuccessful))
            {
                AssetDeliveryErrorCode errorCode = assetPackSizeOperations
                    .Where(operation => !operation.IsSuccessful)
                    .Select(op => op.Error)
                    .FirstOrDefault();
                Debug.LogFormat("[{0}.{1}] errorCode={2}", nameof(DownloadSizeAsyncOperation), nameof(OnGetPackDownloadSize), errorCode);
                Complete(AsyncOperationStatus.Failed, new Exception($"Failed to retrieve pending assetPack size {errorCode}"));
                return;
            }
            Result = assetPackSizeOperations.Sum(operation => operation.GetResult());
            Debug.LogFormat("[{0}.{1}] downloadSize={2}", nameof(DownloadSizeAsyncOperation), nameof(OnGetPackDownloadSize), Result);
            Complete(AsyncOperationStatus.Succeeded);
        }

        private void Complete(AsyncOperationStatus status, Exception exception = null)
        {
            Status = status;
            OperationException = exception;
            IsDone = true;
        }
    }
}
#endif