using System;

namespace Submodules.Common.Tools.Overlays
{
    [Serializable]
    public abstract class ScriptIconRule
    {
        public abstract bool Matches(ScriptRuleContext ctx);
        public abstract ScriptIconElement GetElement(ScriptRuleContext ctx);
    }
}
