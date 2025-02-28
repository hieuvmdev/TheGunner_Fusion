using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtils
{

#if BUILD_LIVE
    public static bool isDebugBuild = false;
#else
    public static bool isDebugBuild = true;
#endif
    static public void Log(string message, bool force = false)
    {

        if (force || isDebugBuild)
        {
#if NETFX_CORE || WINDOWS_PHONE || UNITY_WP8
            System.Diagnostics.Debug.WriteLine(message);
#else
            Debug.Log(message);
#endif
        }

    }
    static public void LogColor(string message, string color, bool force = false)
    {
        if (force || isDebugBuild)
        {
            Debug.Log(string.Format("<color={0}>" + message + "</color>", color));
        }
    }

    static public void LogColorDetail(string messageColor, string message, string color, bool force = false)
    {
        if (force || isDebugBuild)
        {
            Debug.Log(string.Format("<color={0}>" + messageColor + "</color> " + message, color));
        }
    }

    static public void LogWarning(string message, bool force = false)
    {
        if (force || isDebugBuild)
        {
            Debug.LogWarning(message);
        }
    }

    static public void LogError(string message, bool force = false)
    {
        if (force || isDebugBuild)
        {
            Debug.LogError(message);
        }
    }
}