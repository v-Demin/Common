using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MyProject.Editor
{
    [CustomPropertyDrawer(typeof(SubClassSelectorAttribute))]
    public class SubClassSelectorDrawer : PropertyDrawer
    {
        private const string ModePrefKey = "SubClassSelector_Mode";
        private const float IconBtnWidth = 25f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
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
                    property.managedReferenceValue = selectedType == null ? null : Activator.CreateInstance(selectedType);
                    property.serializedObject.ApplyModifiedProperties();
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

            EditorGUI.PropertyField(position, property, GUIContent.none, true);
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

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) 
            => EditorGUI.GetPropertyHeight(property, label, true);

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

    public class SubClassTypeDropdown : AdvancedDropdown
    {
        private readonly IEnumerable<Type> _types;
        private readonly Action<Type> _onSelected;
        private readonly string _title;

        public SubClassTypeDropdown(AdvancedDropdownState state, IEnumerable<Type> types, string title, Action<Type> onSelected) : base(state)
        {
            _types = types; _onSelected = onSelected; _title = title;
            minimumSize = new Vector2(300, 400);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(_title);
            root.AddChild(new TypeDropdownItem(null, "None (Null)"));

            int mode = EditorPrefs.GetInt("SubClassSelector_Mode", 0);
            if (mode == 1) BuildNamespaceTree(root);
            else if (mode == 2) BuildInheritanceTree(root);
            else foreach (var t in _types.OrderBy(t => t.Name)) root.AddChild(new TypeDropdownItem(t));
            
            return root;
        }

        private void BuildNamespaceTree(AdvancedDropdownItem root)
        {
            foreach (var type in _types.OrderBy(t => t.FullName))
            {
                string ns = type.Namespace ?? "{Global}";
                string[] parts = ns.Split('.');
                AdvancedDropdownItem current = root;
                foreach (var part in parts)
                {
                    var found = current.children.FirstOrDefault(c => c.name == part);
                    if (found == null) { found = new AdvancedDropdownItem(part); current.AddChild(found); }
                    current = found;
                }
                current.AddChild(new TypeDropdownItem(type));
            }
        }

        private void BuildInheritanceTree(AdvancedDropdownItem root)
        {
            var nodes = new Dictionary<Type, AdvancedDropdownItem>();
            foreach (var type in _types)
            {
                List<Type> hierarchy = new List<Type>();
                Type curr = type;
                while (curr != null && curr != typeof(object)) { hierarchy.Add(curr); curr = curr.BaseType; }
                hierarchy.Reverse();
                AdvancedDropdownItem parent = root;
                foreach (var t in hierarchy)
                {
                    if (!nodes.TryGetValue(t, out var node)) { node = new TypeDropdownItem(t); parent.AddChild(node); nodes[t] = node; }
                    parent = node;
                }
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is TypeDropdownItem typeItem) _onSelected?.Invoke(typeItem.Type);
        }

        private class TypeDropdownItem : AdvancedDropdownItem
        {
            public Type Type { get; }
            public TypeDropdownItem(Type type, string name = null) : base(name ?? type.Name)
            {
                Type = type;
                if (type != null) icon = (Texture2D)EditorGUIUtility.ObjectContent(null, type).image;
            }
        }
    }
}