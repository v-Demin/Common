using UnityEditor;
using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
    public class PathIconElement : ScriptIconElement
    {
        private readonly string _path;
        private Texture2D _cachedTexture;

        public PathIconElement(string path)
        {
            _path = path;
        }

        public override void Draw(Rect rect)
        {
            if (_cachedTexture == null && !string.IsNullOrEmpty(_path))
            {
                // 1. Пытаемся загрузить из Resources
                _cachedTexture = Resources.Load<Texture2D>(_path);

                // 2. Если путь проектный, ищем через AssetDatabase
                if (_cachedTexture == null && _path.StartsWith("Assets/"))
                {
                    _cachedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_path);
                }

                // 3. Если ничего не подошло, пробуем встроенные иконки Unity
                if (_cachedTexture == null)
                {
                    _cachedTexture = EditorGUIUtility.IconContent(_path)?.image as Texture2D;
                }
            }

            if (_cachedTexture != null)
            {
                GUI.DrawTexture(rect, _cachedTexture);
            }
        }
    }
}
