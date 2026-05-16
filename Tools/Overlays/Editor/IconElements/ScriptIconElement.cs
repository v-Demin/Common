using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
    public abstract class ScriptIconElement
    {
        public virtual float Width => 14f;
        public abstract void Draw(Rect rect);
    }
}
