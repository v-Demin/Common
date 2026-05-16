using System;
using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
    [Serializable]
    public class MonoBehaviourRule : ScriptIconRule
    {
        private const string IconPath = "MonoScriptIcon";

        public override bool Matches(ScriptRuleContext ctx)
        {
            var type = ctx.Type;
            if (type == null) return false;

            return typeof(MonoBehaviour).IsAssignableFrom(type);
        }

        public override ScriptIconElement GetElement(ScriptRuleContext ctx)
        {
            return new PathIconElement(IconPath);
        }
    }
}
