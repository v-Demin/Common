using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public class SubclassCreator
{
    [MenuItem("Assets/Create/C# Subclass", false, 10)]
    private static void CreateSubclass()
    {
        MonoScript selectedScript = Selection.activeObject as MonoScript;
        if (selectedScript == null) return;

        Type parentType = selectedScript.GetClass();
        if (parentType == null) return;

        string baseClassName = selectedScript.name;
        string ns = parentType.Namespace;

        // Находим путь к нашему шаблону .txt
        string[] guids = AssetDatabase.FindAssets("SubclassTemplate t:TextAsset");
        if (guids.Length == 0)
        {
            Debug.LogError("Не найден файл SubclassTemplate.txt!");
            return;
        }
        string templatePath = AssetDatabase.GUIDToAssetPath(guids[0]);

        // Подготавливаем содержимое (заменяем кастомные токены заранее)
        string templateContent = File.ReadAllText(templatePath);
        
        if (!string.IsNullOrEmpty(ns))
        {
            templateContent = templateContent.Replace("#NAMESPACE_START#", $"namespace {ns}\n{{");
            templateContent = templateContent.Replace("#NAMESPACE_END#", "}");
        }
        else
        {
            templateContent = templateContent.Replace("#NAMESPACE_START#", "");
            templateContent = templateContent.Replace("#NAMESPACE_END#", "");
        }
        
        templateContent = templateContent.Replace("#PARENTNAME#", baseClassName);

        // Создаем временный файл шаблона с уже подставленным namespace и родителем
        string tempTemplatePath = Path.Combine(Path.GetTempPath(), "TempSubclass.cs.txt");
        File.WriteAllText(tempTemplatePath, templateContent);

        // Вызываем стандартное окно создания ассета Unity
        // #SCRIPTNAME# заменится автоматически на то, что ты введешь в поле имени
        ProjectWindowUtil.CreateScriptAssetFromTemplateFile(tempTemplatePath, $"New{baseClassName}.cs");
    }

    [MenuItem("Assets/Create/C# Subclass", true)]
    private static bool CreateSubclassValidation() => Selection.activeObject is MonoScript;
}