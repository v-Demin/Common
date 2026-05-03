using System;
using UnityEngine;

[Serializable]
public abstract class ScriptIconRule
{
    public abstract bool Matches(ScriptRuleContext ctx);
    public abstract Texture2D Icon { get; }
}