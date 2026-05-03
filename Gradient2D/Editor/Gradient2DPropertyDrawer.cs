using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Gradient2D))]
public class Gradient2DPropertyDrawer : PropertyDrawer
{
    private Texture2D fullPreviewTex;
    private Texture2D thumbnailTex;
    private int lastVersion = -1;

    private int selectedIndex = -1;
    private bool isDraggingKey = false;
    private bool isDraggingSampled = false;
    private Vector2? sampledUV = null;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight + 4f;

        float height = EditorGUIUtility.singleLineHeight + 170f;

        if (selectedIndex >= 0 || sampledUV.HasValue)
            height += EditorGUIUtility.singleLineHeight * 3f + 12f;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Gradient2D gradient = GetGradient(property);
        if (gradient == null) return;

        Event e = Event.current;
        float lineHeight = EditorGUIUtility.singleLineHeight;

        EditorGUI.BeginProperty(position, label, property);

        Rect foldoutRect = new Rect(position.x, position.y, lineHeight, lineHeight);
        bool expanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none);

        Rect labelRect = new Rect(position.x + lineHeight + 2, position.y, EditorGUIUtility.labelWidth - lineHeight, lineHeight);
        EditorGUI.LabelField(labelRect, label);

        Rect clickableRect = new Rect(position.x + lineHeight, position.y, position.width - lineHeight, lineHeight);
        if (e.type == EventType.MouseDown && clickableRect.Contains(e.mousePosition) && e.button == 0)
        {
            expanded = !expanded;
            e.Use();
        }

        property.isExpanded = expanded;

        if (!expanded)
        {
            Rect thumbRect = new Rect(position.x + EditorGUIUtility.labelWidth + 6f, position.y + 1f,
                                      position.width - EditorGUIUtility.labelWidth - 12f, lineHeight - 2f);
            DrawThumbnail(thumbRect, gradient);
            EditorGUI.EndProperty();
            return;
        }

        float y = position.y + lineHeight + 6f;

        Rect previewRect = new Rect(position.x, y, position.width, 160f);
        DrawPreview(previewRect, property, gradient, e);
        y += 164f;

        if (selectedIndex >= 0)
        {
            SerializedProperty keysProp = property.FindPropertyRelative("keys");
            if (selectedIndex < keysProp.arraySize)
            {
                SerializedProperty keyProp = keysProp.GetArrayElementAtIndex(selectedIndex);
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), keyProp.FindPropertyRelative("position"), new GUIContent("Position"));
                y += lineHeight + 4f;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), keyProp.FindPropertyRelative("color"), new GUIContent("Color"));
                y += lineHeight + 4f;
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lineHeight), keyProp.FindPropertyRelative("power"), new GUIContent("Power"));
            }
        }
        else if (sampledUV.HasValue)
        {
            Color sampledColor = gradient.Evaluate(sampledUV.Value);
            EditorGUI.LabelField(new Rect(position.x, y, 120f, lineHeight), "Sampled Color");
            EditorGUI.ColorField(new Rect(position.x + 120f, y, position.width - 120f, lineHeight), GUIContent.none, sampledColor, false, false, false);
            y += lineHeight + 4f;
            EditorGUI.LabelField(new Rect(position.x, y, position.width, lineHeight), $"Position: {sampledUV.Value.x:F2}, {sampledUV.Value.y:F2}");
        }

        if (GUI.changed)
        {
            gradient.version++;
            property.serializedObject.ApplyModifiedProperties();
        }

        EditorGUI.EndProperty();
    }

    private void DrawPreview(Rect rect, SerializedProperty property, Gradient2D gradient, Event e)
    {
        if (fullPreviewTex == null || fullPreviewTex.width != 128)
        {
            fullPreviewTex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            fullPreviewTex.filterMode = FilterMode.Bilinear;
        }

        if (gradient.version != lastVersion)
        {
            UpdatePreviewTexture(fullPreviewTex, gradient);
            lastVersion = gradient.version;
        }

        GUI.DrawTexture(rect, fullPreviewTex, ScaleMode.StretchToFill);

        Vector2 mouseUV = GetMouseUV(rect, e.mousePosition);
        HandleInteractions(rect, property, gradient, mouseUV, e);

        // Точки с двойной контрастной обводкой
        if (Event.current.type == EventType.Repaint)
        {
            Handles.BeginGUI();
            for (int i = 0; i < gradient.keys.Count; i++)
            {
                var key = gradient.keys[i];
                Vector2 screenCenter = rect.position + new Vector2(key.position.x * rect.width, (1f - key.position.y) * rect.height);
                float pointSize = (i == selectedIndex) ? 9.5f : 6.5f;

                Handles.color = Color.black;
                Handles.DrawSolidDisc(screenCenter, Vector3.forward, pointSize + 3.5f);
                Handles.color = Color.white;
                Handles.DrawSolidDisc(screenCenter, Vector3.forward, pointSize + 1.8f);
                Handles.color = key.color;
                Handles.DrawSolidDisc(screenCenter, Vector3.forward, pointSize);
            }
            Handles.EndGUI();
        }

        // Sampled точка (чёрно-белая)
        if (sampledUV.HasValue)
        {
            Vector2 screenPos = rect.position + new Vector2(sampledUV.Value.x * rect.width, (1f - sampledUV.Value.y) * rect.height);
            if (Event.current.type == EventType.Repaint)
            {
                Handles.BeginGUI();
                Handles.color = Color.white;
                Handles.DrawSolidDisc(screenPos, Vector3.forward, 5f);
                Handles.color = Color.black;
                Handles.DrawSolidDisc(screenPos, Vector3.forward, 3f);
                Handles.EndGUI();
            }
        }
    }

    private void DrawThumbnail(Rect rect, Gradient2D gradient)
    {
        if (thumbnailTex == null || thumbnailTex.width != 96 || thumbnailTex.height != 18)
            thumbnailTex = new Texture2D(96, 18, TextureFormat.RGBA32, false);

        UpdatePreviewTexture(thumbnailTex, gradient);
        GUI.DrawTexture(rect, thumbnailTex, ScaleMode.StretchToFill);
    }

    private void HandleInteractions(Rect rect, SerializedProperty property, Gradient2D gradient, Vector2 mouseUV, Event e)
    {
        if (!rect.Contains(e.mousePosition)) return;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    int closest = GetClosestKeyIndex(gradient, mouseUV);

                    // Очень точный клик по точке (маленькая зона)
                    if (closest != -1 && Vector2.Distance(gradient.keys[closest].position, mouseUV) < 0.045f)
                    {
                        selectedIndex = closest;
                        sampledUV = null;
                        isDraggingKey = true;
                        isDraggingSampled = false;
                    }
                    else
                    {
                        // Клик в пустое место → ставим sampled и сразу начинаем drag
                        selectedIndex = -1;
                        sampledUV = mouseUV;
                        isDraggingKey = false;
                        isDraggingSampled = true;
                    }
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (isDraggingKey && selectedIndex >= 0)
                {
                    Vector2 clamped = new Vector2(Mathf.Clamp01(mouseUV.x), Mathf.Clamp01(mouseUV.y));
                    var k = gradient.keys[selectedIndex];
                    gradient.keys[selectedIndex] = new Gradient2D.Key(clamped, k.color, k.power);
                    gradient.version++;
                    property.serializedObject.ApplyModifiedProperties();
                    e.Use();
                }
                else if (isDraggingSampled && sampledUV.HasValue)
                {
                    // Drag sampled point
                    sampledUV = new Vector2(Mathf.Clamp01(mouseUV.x), Mathf.Clamp01(mouseUV.y));
                    gradient.version++; // обновляем превью
                    property.serializedObject.ApplyModifiedProperties();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                isDraggingKey = false;
                isDraggingSampled = false;
                break;

            case EventType.ContextClick:
                int closestCtx = GetClosestKeyIndex(gradient, mouseUV);
                if (closestCtx != -1 && Vector2.Distance(gradient.keys[closestCtx].position, mouseUV) < 0.045f)
                {
                    gradient.keys.RemoveAt(closestCtx);
                    if (selectedIndex >= gradient.keys.Count) selectedIndex = -1;
                }
                else
                {
                    gradient.keys.Add(new Gradient2D.Key(mouseUV, Color.white, 2.5f));
                    selectedIndex = gradient.keys.Count - 1;
                    sampledUV = null;
                }
                gradient.version++;
                property.serializedObject.ApplyModifiedProperties();
                e.Use();
                break;
        }
    }

    private int GetClosestKeyIndex(Gradient2D gradient, Vector2 uv)
    {
        int closest = -1;
        float minDist = float.MaxValue;
        for (int i = 0; i < gradient.keys.Count; i++)
        {
            float d = Vector2.Distance(gradient.keys[i].position, uv);
            if (d < minDist)
            {
                minDist = d;
                closest = i;
            }
        }
        return closest;
    }

    private Vector2 GetMouseUV(Rect rect, Vector2 mousePos)
    {
        Vector2 local = (mousePos - rect.position) / rect.size;
        return new Vector2(Mathf.Clamp01(local.x), Mathf.Clamp01(1f - local.y));
    }

    private void UpdatePreviewTexture(Texture2D tex, Gradient2D gradient)
    {
        Color[] pixels = new Color[tex.width * tex.height];
        int idx = 0;
        for (int y = 0; y < tex.height; y++)
            for (int x = 0; x < tex.width; x++)
            {
                Vector2 uv = new Vector2((float)x / (tex.width - 1), (float)y / (tex.height - 1));
                pixels[idx++] = gradient.Evaluate(uv);
            }
        tex.SetPixels(pixels);
        tex.Apply();
    }

    private Gradient2D GetGradient(SerializedProperty property)
    {
        return fieldInfo.GetValue(property.serializedObject.targetObject) as Gradient2D;
    }
}