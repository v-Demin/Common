using System;

namespace Submodules.Common.Tools.Overlays
{
    [Serializable]
    public class AbstractClassRule : ScriptIconRule
    {
        private const string IconPath = "AbstractScriptIcon";

        public override bool Matches(ScriptRuleContext ctx)
        {
            var t = ctx.Type;
            return t is { IsAbstract: true, IsSealed: false };
        }

        public override ScriptIconElement GetElement(ScriptRuleContext ctx)
        {
            return new PathIconElement(IconPath);
        }
    }
}
