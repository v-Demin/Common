using UnityEngine;

public static class DebugExtensions
{

    public static void LogBool(this string message)
    {
        message.LogBool(message.Contains("True"));
    }
    public static void LogBool(this string message, bool isTrue)
    {
        message.Log(isTrue ? Color.green : Color.red);
    }
    
    public static void Log(this string message, Color color)
    {
        Debug.LogFormat($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
    }
    
    public static void Log<T>(this T target, Color color)
    {
        Debug.LogFormat($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{target}</color>");
    }

    public static void LogTarget<T>(this T target, string message, Color color) where T : Object
    {
        Debug.LogFormat(target, $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{message}</color>");
    }
}
