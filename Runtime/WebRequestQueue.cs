using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Khepri.AssetDelivery
{
    internal class WebRequestQueueOperation
    {
        public UnityWebRequestAsyncOperation Result;
        public Action<UnityWebRequestAsyncOperation> OnComplete;

        public bool IsDone => Result != null;

        internal UnityWebRequest m_WebRequest;

        public WebRequestQueueOperation(UnityWebRequest request)
        {
            m_WebRequest = request;
        }

        internal void Complete(UnityWebRequestAsyncOperation asyncOp)
        {
            Result = asyncOp;
            OnComplete?.Invoke(Result);
        }
    }
    
    /*
     * Copy of the internal classof U nityEngine.ResourceManagement.
     */
    internal static class WebRequestQueue
    {
        private static int s_MaxRequest = 500;
        private static Queue<WebRequestQueueOperation> s_QueuedOperations = new Queue<WebRequestQueueOperation>();
        private static List<UnityWebRequestAsyncOperation> s_ActiveRequests = new List<UnityWebRequestAsyncOperation>();

        public static WebRequestQueueOperation QueueRequest(UnityWebRequest request)
        {
            WebRequestQueueOperation queueOperation = new WebRequestQueueOperation(request);
            if (s_ActiveRequests.Count < s_MaxRequest)
            {
                var webRequestAsyncOp = request.SendWebRequest();
                webRequestAsyncOp.completed += OnWebAsyncOpComplete;
                s_ActiveRequests.Add(webRequestAsyncOp);
                queueOperation.Complete(webRequestAsyncOp);
            }
            else
                s_QueuedOperations.Enqueue(queueOperation);

            return queueOperation;
        }

        private static void OnWebAsyncOpComplete(AsyncOperation operation)
        {
            s_ActiveRequests.Remove(operation as UnityWebRequestAsyncOperation);

            if (s_QueuedOperations.Count == 0) return;
            
            var nextQueuedOperation = s_QueuedOperations.Dequeue();
            var webRequestAsyncOp = nextQueuedOperation.m_WebRequest.SendWebRequest();
            webRequestAsyncOp.completed += OnWebAsyncOpComplete;
            s_ActiveRequests.Add(webRequestAsyncOp);
            nextQueuedOperation.Complete(webRequestAsyncOp);
        }
    }
}