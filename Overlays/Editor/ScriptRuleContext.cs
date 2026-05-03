using System;
using UnityEditor;

public readonly struct ScriptRuleContext
{
    public readonly string Path;
    public readonly Type Type;
    public readonly MonoScript Script;

    public ScriptRuleContext(string path, MonoScript script, Type type)
    {
        Path = path;
        Script = script;
        Type = type;
    }
}