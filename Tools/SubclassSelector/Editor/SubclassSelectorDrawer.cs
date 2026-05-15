using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Submodules.Common.Tools.SubclassSelector
{
    [CustomPropertyDrawer(typeof(SubClassSelectorAttribute))]
    public class SubClassSelectorDrawer : PropertyDrawer
    {
        private const string ModePrefKey = "SubClassSelector_Mode";
        private const float IconBtnWidth = 25f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ПРЕДОХРАНИТЕЛЬ: Если проект компилируется, просто рисуем заглушку.
            // Это не даст инспектору Unity лезть в нативные типы твоего asmdef во время пересборки
            // и полностью предотвратит появление ошибки "invalid GC handle".
            if (EditorApplication.isCompiling)
            {
                EditorGUI.LabelField(position, label.text, "Compiling asmdef...");
                return;
            }

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.LabelField(position, label.text, "Use [SubClassSelector] only with [SerializeReference]");
                return;
            }

            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            Rect settingsBtnRect = new Rect(position.xMax - IconBtnWidth, position.y, IconBtnWidth, EditorGUIUtility.singleLineHeight);
            Rect pingBtnRect = new Rect(settingsBtnRect.x - IconBtnWidth, position.y, IconBtnWidth, EditorGUIUtility.singleLineHeight);
            
            float typeBtnWidth = position.width - EditorGUIUtility.labelWidth - (IconBtnWidth * 2) - 4f;
            Rect typeBtnRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, typeBtnWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.HandlePrefixLabel(position, labelRect, label, 0);

            string fullType = property.managedReferenceFullTypename;
            string typeName = string.IsNullOrEmpty(fullType) ? "None (Null)" : fullType.Split(' ').Last().Split('.').Last();
            
            if (GUI.Button(typeBtnRect, new GUIContent(typeName, fullType), EditorStyles.popup))
            {
                Type baseType = GetFieldType(property);
                var types = TypeCache.GetTypesDerivedFrom(baseType)
                    .Where(t => !t.IsAbstract && !t.IsInterface && t.IsSerializable);

                var state = new AdvancedDropdownState();
                var dropdown = new SubClassTypeDropdown(state, types, baseType.Name, (selectedType) =>
                {
                    // Важно: меняем значение через Undo, чтобы Unity корректно регистрировала изменения в нативной части
                    Undo.RecordObject(property.serializedObject.targetObject, "Change SubClass Type");
                    
                    property.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
                    property.serializedObject.ApplyModifiedProperties();
                    
                    // Насильно маркируем объект грязным и сохраняем, чтобы кэш памяти не затерся при компиляции
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                    AssetDatabase.SaveAssets();
                });
                
                dropdown.Show(typeBtnRect);
            }

            GUI.enabled = !string.IsNullOrEmpty(fullType);
            var searchIcon = EditorGUIUtility.IconContent("Search Icon") ?? EditorGUIUtility.IconContent("d_SearchIcon") ?? new GUIContent("L");

            if (GUI.Button(pingBtnRect, searchIcon, EditorStyles.miniButton))
            {
                PingScriptAsset(fullType);
            }
            GUI.enabled = true;

            if (GUI.Button(settingsBtnRect, EditorGUIUtility.IconContent("SettingsIcon"), EditorStyles.miniButton))
            {
                GenericMenu menu = new GenericMenu();
                AddModeOption(menu, "Flat List", 0);
                AddModeOption(menu, "By Namespace", 1);
                AddModeOption(menu, "By Inheritance", 2);
                menu.ShowAsContext();
            }

            // Стандартная отрисовка полей самого класса
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
        {
            if (EditorApplication.isCompiling)
                return EditorGUIUtility.singleLineHeight;
                
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private void PingScriptAsset(string managedReferenceFullTypename)
        {
            var parts = managedReferenceFullTypename.Split(' ');
            if (parts.Length < 2) return;
            Type type = Type.GetType($"{parts[1]}, {parts[0]}");
            if (type == null) return;

            var guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    EditorGUIUtility.PingObject(script);
                    return;
                }
            }
        }

        private void AddModeOption(GenericMenu menu, string name, int mode)
        {
            menu.AddItem(new GUIContent(name), EditorPrefs.GetInt(ModePrefKey, 0) == mode, () => EditorPrefs.SetInt(ModePrefKey, mode));
        }

        private Type GetFieldType(SerializedProperty property)
        {
            string[] path = property.propertyPath.Split('.');
            Type type = property.serializedObject.targetObject.GetType();
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] == "Array") { i++; type = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType(); continue; }
                FieldInfo field = type.GetField(path[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (field != null) type = field.FieldType;
            }
            return type;
        }
    }
}