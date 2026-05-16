using System;
using System.Collections.Generic;
using Submodules.Common.Tools.SubclassSelector;
using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
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

        public List<ScriptIconElement> GetElements(ScriptRuleContext ctx)
        {
            if (Rules == null) return null;
            List<ScriptIconElement> result = null;

            for (int i = 0; i < Rules.Count; i++)
            {
                var rule = Rules[i];
                if (rule == null) continue;
                if (!rule.Matches(ctx)) continue;

                result ??= new List<ScriptIconElement>();
                var element = rule.GetElement(ctx);
                if (element != null)
                {
                    result.Add(element);
                }
            }

            return result;
        }
    }
}

