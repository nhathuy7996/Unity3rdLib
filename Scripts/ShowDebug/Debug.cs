
using UnityEngine;
using DVAH;
using System;
using Object = UnityEngine.Object;

public class Debug : UnityEngine.Debug
{
    private static string _hr = "";

    public static new void Log(object message)
    {
#if PRODUCTION
            return;
#endif
        UnityEngine.Debug.Log(message.ToString() + _hr);
        if (NotiManager.Instant != null)
            NotiManager.Instant.Log("[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + message.ToString() + _hr);
    }

    public static new void Log(object message, Object context)
    {
#if PRODUCTION
            return;
#endif
        UnityEngine.Debug.Log(message.ToString() + _hr, context);
        if (NotiManager.Instant != null)
            NotiManager.Instant.Log("[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + message.ToString() + _hr);
    }

    public static new void LogError(object message)
    {
#if PRODUCTION
            return;
#endif
        UnityEngine.Debug.LogError(message.ToString() + _hr);
        if (NotiManager.Instant != null)
            NotiManager.Instant.Log("<color=red>[" + DateTime.Now.ToString("h:mm:ss tt") + "] " + message.ToString() + _hr + "</color>");

    }

    public static new void LogError(object message, Object context)
    {
#if PRODUCTION
            return;
#endif
        UnityEngine.Debug.LogError(message.ToString() + _hr, context);
        if (NotiManager.Instant != null)
            NotiManager.Instant.Log("<color=red>[" + DateTime.Now.ToString("h: mm:ss tt") + "] " + message.ToString() + _hr + " </color>");
    }

    public static new void LogWarning(object message)
    {
#if PRODUCTION
            return;
#endif
        UnityEngine.Debug.LogWarning(message.ToString() + _hr);
        if (NotiManager.Instant != null)
            NotiManager.Instant.Log("<color=yellow>[" + DateTime.Now.ToString("h: mm:ss tt") + "] " + message.ToString() + _hr + " </color>");
    }

    public static new void LogWarning(object message, Object context)
    {
#if PRODUCTION
            return;
#endif
        UnityEngine.Debug.LogWarning(message.ToString() + _hr, context);
        if (NotiManager.Instant != null)
            NotiManager.Instant.Log("<color=yellow>[" + DateTime.Now.ToString("h: mm:ss tt") + "] " + message.ToString() + _hr + " </color>");
    }
}
