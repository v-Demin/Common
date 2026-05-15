using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ScriptIconProjectWindowDrawer
{
    private static ScriptIconRuleSet _ruleSet;
    private static readonly Dictionary<string, List<Texture2D>> Cache = new();

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
        // Если статика слетела после компиляции — ищем ассет в проекте
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

        if (!Cache.TryGetValue(guid, out var icons))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            // Работаем только с C# скриптами
            if (!path.EndsWith(".cs")) return;

            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            if (script != null)
            {
                var type = script.GetClass();
                if (type != null)
                {
                    var ctx = new ScriptRuleContext(path, script, type);
                    icons = _ruleSet.GetIcons(ctx);
                    Cache[guid] = icons;
                }
            }
        }

        if (icons != null) Draw(rect, icons);
    }

    private static void Draw(Rect rect, List<Texture2D> icons)
    {
        if (icons.Count == 0) return;

        const float size = 14f;
        const float spacing = 2f;
        float x = rect.xMax - size;
        float y = rect.yMax - size;

        // Коррекция для режима списка (маленькие иконки)
        if (rect.height < 20) y = rect.y;

        for (int i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];
            if (icon == null) continue;
            GUI.DrawTexture(new Rect(x, y, size, size), icon);
            x -= (size + spacing);
        }
    }
}