using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
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

            // Сбрасываем кэш при компиляции, чтобы новые asmdef/скрипты подтягивались сразу
            CompilationPipeline.compilationStarted += (_) => Cache.Clear();
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
                if (string.IsNullOrEmpty(path)) return;

                if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script == null) return;

                    var type = script.GetClass();
                    // Если скрипт пустой или сломан — пока пропускаем, либо закладываем фоллбэк.
                    // Но для компилируемых asmref/asmdef тут ВСЕГДА будет валидный тип.
                    if (type != null && type.Assembly != null)
                    {
                        // Твоя родная строка. Для скрипта в asmref она вернет имя ТЕМАТИЧЕСКОГО asmdef!
                        string assemblyName = type.Assembly.GetName().Name;

                        if (assemblyName.EndsWith(".Editor"))
                        {
                            assemblyName = assemblyName.Substring(0, assemblyName.Length - 7);
                        }

                        var ctx = new ScriptRuleContext(path, script, type, assemblyName);
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
            const float defaultHeight = 14f; // Высота твоей иконки (измени на 16f, если нужно)

            // Твой оригинальный рабочий просчет ширины блока
            float totalWidth = 0f;
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] == null) continue;
                totalWidth += elements[i].Width;
                if (i < elements.Count - 1)
                {
                    totalWidth += spacing;
                }
            }

            float x = rect.xMax - totalWidth;
            float y = rect.yMax - defaultHeight;

            if (rect.height < 20) y = rect.y;

            // Твой оригинальный рабочий цикл отрисовки слева направо
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element == null) continue;

                float width = element.Width;
                Rect elementRect = new Rect(x, y, width, defaultHeight);

                element.Draw(elementRect);

                x += width + spacing;
            }
        }
    }
}