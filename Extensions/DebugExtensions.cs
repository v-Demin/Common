using UnityEngine;

public static class DebugExtensions
{
    
    public static void Log(this string message, Color color)
    {
        Debug.LogFormat($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
    }
    
    public static void Log<T>(this T target, Color color) where T : Object
    {
        Debug.LogFormat(target, $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{target.ToString()}</color>");
    }

    public static void Log<T>(this T target, string message, Color color) where T : Object
    {
        Debug.LogFormat(target, $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
    }
}
