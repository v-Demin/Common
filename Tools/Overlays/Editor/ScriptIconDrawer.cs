using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
    [InitializeOnLoad]
    public static class ScriptIconProjectWindowDrawer
    {
        private static ScriptIconRuleSet _ruleSet;
        private static readonly Dictionary<string, List<ScriptIconElement>> Cache = new();

        static ScriptIconProjectWindowDrawer()
        {
            EditorApplication.projectWindowItemOnGUI += OnGUI;
            ScriptIconRuleSet.OnChanged += () => Cache.Clear();
        }

        public static void SetRuleSet(ScriptIconRuleSet set)
        {
            _ruleSet = set;
            Cache.Clear();
        }

        private static void OnGUI(string guid, Rect rect)
        {
            if (_ruleSet == null)
            {
                var assets = AssetDatabase.FindAssets("t:ScriptIconRuleSet");
                if (assets.Length > 0)
                {
                    var path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    _ruleSet = AssetDatabase.LoadAssetAtPath<ScriptIconRuleSet>(path);
                }

                if (_ruleSet == null) return;
            }

            if (!Cache.TryGetValue(guid, out var elements))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".cs")) return;

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null)
                {
                    var type = script.GetClass();
                    if (type != null)
                    {
                        var ctx = new ScriptRuleContext(path, script, type);
                        elements = _ruleSet.GetElements(ctx);
                        Cache[guid] = elements;
                    }
                }
            }

            if (elements != null) Draw(rect, elements);
        }

        private static void Draw(Rect rect, List<ScriptIconElement> elements)
        {
            if (elements == null || elements.Count == 0) return;

            const float spacing = 2f;
            const float defaultHeight = 14f;

            // Высчитываем общую ширину для правильного позиционирования слева-направо
            float totalWidth = 0f;
            for (int i = 0; i < elements.Count; i++)
            {
                totalWidth += elements[i].Width;
                if (i < elements.Count - 1)
                {
                    totalWidth += spacing;
                }
            }

            // Стартовая точка x (левый край блока иконок)
            float x = rect.xMax - totalWidth;
            float y = rect.yMax - defaultHeight;

            if (rect.height < 20) y = rect.y;

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element == null) continue;

                float width = element.Width;
                Rect elementRect = new Rect(x, y, width, defaultHeight);

                element.Draw(elementRect);

                // Сдвигаемся вправо для следующего элемента
                x += width + spacing;
            }
        }
    }
}
