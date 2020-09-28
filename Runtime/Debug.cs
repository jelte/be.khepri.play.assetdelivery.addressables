using System.Diagnostics;

namespace Khepri.AddressableAssets
{
    public class Debug
    {
        [Conditional("DEBUG_ADDRESSABLES")]
        public static void LogFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }
    }
}