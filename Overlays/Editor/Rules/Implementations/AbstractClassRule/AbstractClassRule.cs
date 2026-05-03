using System;
using UnityEditor;
using UnityEngine;

[Serializable]
public class AbstractClassRule : ScriptIconRule
{
    private static Texture2D _icon;

    public override bool Matches(ScriptRuleContext ctx)
    {
        var t = ctx.Type;
        return t is { IsAbstract: true, IsSealed: false };
    }

    public override Texture2D Icon
    {
        get
        {
            if (_icon != null) return _icon;

            _icon =
                Resources.Load<Texture2D>("AbstractScriptIcon")
                ?? (Texture2D)EditorGUIUtility.IconContent("console.warnicon").image;

            return _icon;
        }
    }
}