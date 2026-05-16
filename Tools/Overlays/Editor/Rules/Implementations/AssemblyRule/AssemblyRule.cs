using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
    [Serializable]
    public class AssemblyTagConfig
    {
        // Используем внутренний тип Unity, заточенный под asmdef
        public UnityEditorInternal.AssemblyDefinitionAsset AsmDefFile;
        public string TagText;
        public Color TagColor = Color.white;
    }

    [Serializable]

    public class AssemblyRule : ScriptIconRule
    {
        private const string AsmDefIconPath = "TagDefScriptIcon";
        private const string AsmRefIconPath = "TagRefScriptIcon";

        private static Sprite _cachedDefIcon;
        private static Sprite _cachedRefIcon;

        [SerializeField] private List<AssemblyTagConfig> _configs = new();

        public override bool Matches(ScriptRuleContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.Path)) return false;

            // Нам интересны только скрипты, asmdef и asmref
            string extension = Path.GetExtension(ctx.Path).ToLower();
            if (extension != ".cs" && extension != ".asmdef" && extension != ".asmref") return false;

            // Находим имя сборки, к которой принадлежит этот файл
            string assemblyName = GetAssemblyNameForPath(ctx.Path);

            // Если файл в стандартной сборке Unity (Assembly-CSharp и т.д.) — игнорируем
            return !string.IsNullOrEmpty(assemblyName) &&
                   !assemblyName.StartsWith("Assembly-CSharp") &&
                   !assemblyName.StartsWith("UnityEngine") &&
                   !assemblyName.StartsWith("UnityEditor");
        }

        public override ScriptIconElement GetElement(ScriptRuleContext ctx)
        {
            _cachedDefIcon ??= Resources.Load<Sprite>(AsmDefIconPath);
            _cachedRefIcon ??= Resources.Load<Sprite>(AsmRefIconPath);

            // Определяем, какую иконку рисовать на скрипте. 
            // Если в пути есть .asmref или мы нашли его выше — ставим реф, иначе деф.
            // (Или оставь свою старую логику определения targetIcon)
            Sprite targetIcon = _cachedDefIcon; 

            if (_configs != null && _configs.Count > 0)
            {
                for (int i = 0; i < _configs.Count; i++)
                {
                    var config = _configs[i];
                    if (config == null || config.AsmDefFile == null) continue;

                    // ПОЛУЧАЕМ РЕАЛЬНОЕ ИМЯ СБОРКИ ИЗ JSON ФАЙЛА .asmdef
                    string configAsmName = string.Empty;
                    try
                    {
                        string assetPath = AssetDatabase.GetAssetPath(config.AsmDefFile);
                        string json = File.ReadAllText(assetPath);
                        var data = JsonUtility.FromJson<AsmDefJsonData>(json);
                        configAsmName = data.name;
                    }
                    catch
                    {
                        configAsmName = config.AsmDefFile.name;
                    }

                    // Сравниваем имя сборки, в которой живет скрипт (через type.Assembly),
                    // с внутренним именем сборки из конфига инспектора!
                    if (configAsmName == ctx.AssemblyName)
                    {
                        return new TagElement(config.TagText, config.TagColor, targetIcon);
                    }
                }
            }

            // Тот самый белый тег, который у тебя сейчас горит
            return new TagElement(string.Empty, Color.white, targetIcon);
        }

// Вспомогательный класс для парсинга имени внутри AssemblyRule
        [Serializable]
        private class AsmDefJsonData
        {
            public string name;
        }

        /// <summary>
        /// Ищет имя сборки для указанного файла, поднимаясь вверх по папкам до ближайшего asmdef/asmref
        /// </summary>
        private string GetAssemblyNameForPath(string assetPath)
        {
            string extension = Path.GetExtension(assetPath).ToLower();

            // Если это сам файл asmdef, то его имя и есть имя сборки
            if (extension == ".asmdef")
            {
                return Path.GetFileNameWithoutExtension(assetPath);
            }

            // Если это сам файл asmref, нам нужно узнать, на какой asmdef он ссылается.
            // Для простоты мы можем взять имя самого файла asmref, если у тебя конфиги настроены под них,
            // либо читать его внутренности. Но обычно скрипты внутри asmref компилируются в имя родительского asmdef.
            // Поэтому идем вверх по папкам:
            string directory = Path.GetDirectoryName(assetPath);

            while (!string.IsNullOrEmpty(directory))
            {
                // Ищем asmdef в текущей папке
                string[] asmDefs = Directory.GetFiles(directory, "*.asmdef");
                if (asmDefs.Length > 0)
                {
                    return Path.GetFileNameWithoutExtension(asmDefs[0]);
                }

                // Ищем asmref в текущей папке. Если нашли — берем его имя
                string[] asmRefs = Directory.GetFiles(directory, "*.asmref");
                if (asmRefs.Length > 0)
                {
                    return Path.GetFileNameWithoutExtension(asmRefs[0]);
                }

                directory = Path.GetDirectoryName(directory);
            }

            // Если вообще ничего не нашли, значит это стандартная сборка (Assembly-CSharp)
            return "Assembly-CSharp";
        }

        private bool CheckIfInsideAsmRef(string assetPath)
        {
            string directory = Path.GetDirectoryName(assetPath);

            while (!string.IsNullOrEmpty(directory))
            {
                if (Directory.GetFiles(directory, "*.asmref").Length > 0) return true;
                if (Directory.GetFiles(directory, "*.asmdef").Length > 0) return false;

                directory = Path.GetDirectoryName(directory);
            }

            return false;
        }
    }
}
