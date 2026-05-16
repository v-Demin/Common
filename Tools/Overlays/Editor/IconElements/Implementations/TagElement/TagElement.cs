using UnityEditor;
using UnityEngine;

namespace Submodules.Common.Tools.Overlays
{
    public class TagElement : ScriptIconElement
    {
        private readonly string _text;
        private readonly Color _color;
        private readonly string _iconPath;

        private Sprite _sprite;
        private float? _cachedWidth;

        private const float FixedHeight = 14f;

        public TagElement(string text, Color color, string iconPath)
        {
            _text = text;
            _color = color;
            _iconPath = iconPath;
        }

        public TagElement(string text, Color color, Sprite sprite)
        {
            _text = text;
            _color = color;
            _sprite = sprite;
            _iconPath = null;
        }

        public override float Width
        {
            get
            {
                if (_cachedWidth.HasValue) return _cachedWidth.Value;

                if (_sprite == null && !string.IsNullOrEmpty(_iconPath))
                {
                    _sprite = Resources.Load<Sprite>(_iconPath)
                              ?? AssetDatabase.LoadAssetAtPath<Sprite>(_iconPath);
                }

                if (_sprite != null && _sprite.texture != null)
                {
                    // Ширина элемента — это ВСЕГДА только натуральная ширина картинки при FixedHeight
                    float aspectRatio = (float)_sprite.texture.width / _sprite.texture.height;
                    _cachedWidth = FixedHeight * aspectRatio;
                }
                else
                {
                    _cachedWidth = 14f;
                }

                return _cachedWidth.Value;
            }
        }

        public override void Draw(Rect rect)
        {
            if (_sprite == null && !string.IsNullOrEmpty(_iconPath))
            {
                _sprite = Resources.Load<Sprite>(_iconPath)
                          ?? AssetDatabase.LoadAssetAtPath<Sprite>(_iconPath);
            }

            if (_sprite == null || _sprite.texture == null) return;

            // 1. ПРОСТО КАРТИНКА (Берет rect, центрируется по вертикали, сохраняет ширину Width)
            float imgY = 1f + rect.y + (rect.height - FixedHeight) / 2f;
            Rect imgRect = new Rect(rect.x, imgY, rect.width, FixedHeight);

            Color oldColor = GUI.color;
            GUI.color = _color;
            GUI.DrawTexture(imgRect, _sprite.texture);
            GUI.color = oldColor;

            // 2. ПРОСТО ТЕКСТ ПОВЕРХ (Плевать на размер rect, пишем по центру этой же зоны)
            if (!string.IsNullOrEmpty(_text))
            {
                var previousAlignment = EditorStyles.miniLabel.alignment;
                var previousClipping = EditorStyles.miniLabel.clipping;

                // Отключаем обрезку (clipping), чтобы текст не пропадал, если он шире картинки
                EditorStyles.miniLabel.alignment = TextAnchor.MiddleCenter;
                EditorStyles.miniLabel.clipping = TextClipping.Overflow;

                GUI.Label(imgRect, _text, EditorStyles.miniLabel);

                EditorStyles.miniLabel.alignment = previousAlignment;
                EditorStyles.miniLabel.clipping = previousClipping;
            }
        }
    }
}
