using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Editor/Script Icon Rule Set")]
public class ScriptIconRuleSet : ScriptableObject
{
    public static event Action OnChanged;

    [SerializeReference] [SubClassSelector] public List<ScriptIconRule> Rules = new();

    private void OnEnable()
    {
        ScriptIconProjectWindowDrawer.SetRuleSet(this);
    }

    private void OnValidate()
    {
        OnChanged?.Invoke();
        ScriptIconProjectWindowDrawer.SetRuleSet(this);
    }

    public List<Texture2D> GetIcons(ScriptRuleContext ctx)
    {
        if (Rules == null) return null;
        List<Texture2D> result = null;

        for (int i = 0; i < Rules.Count; i++)
        {
            var rule = Rules[i];
            if (rule == null) continue;
            if (!rule.Matches(ctx)) continue;

            result ??= new List<Texture2D>();
            result.Add(rule.Icon);
        }

        return result;
    }
}