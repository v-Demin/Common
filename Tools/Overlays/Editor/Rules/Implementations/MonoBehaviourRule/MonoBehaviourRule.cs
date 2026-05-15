using System;
using UnityEngine;
using UnityEditor;

[Serializable]
public class MonoBehaviourRule : ScriptIconRule
{
    private static Texture2D _icon;

    public override bool Matches(ScriptRuleContext ctx)
    {
        var type = ctx.Type;
        if (type == null) return false;

        return typeof(MonoBehaviour).IsAssignableFrom(type);
    }

    public override Texture2D Icon
    {
        get
        {
            if (_icon != null) return _icon;
            _icon = Resources.Load<Texture2D>("MonoScriptIcon")
                    ?? (Texture2D)EditorGUIUtility.IconContent("cs Script Icon").image;
            return _icon;
        }
    }
}