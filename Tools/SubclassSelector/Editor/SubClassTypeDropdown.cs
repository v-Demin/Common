using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Submodules.Common.Tools.SubclassSelector
{
    public class SubClassTypeDropdown : AdvancedDropdown
    {
        private readonly IEnumerable<Type> _types;
        private readonly Action<Type> _onSelected;
        private readonly string _title;

        public SubClassTypeDropdown(AdvancedDropdownState state, IEnumerable<Type> types, string title, Action<Type> onSelected) : base(state)
        {
            _types = types; 
            _onSelected = onSelected; 
            _title = title;
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