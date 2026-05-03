using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Gradient2D
{
    [System.Serializable]
    public struct Key
    {
        public Vector2 position;
        public Color color;

        [Range(0.5f, 8f)]
        [Tooltip("Сила/падение цвета для этой точки (чем выше — тем резче переход)")]
        public float power;

        public Key(Vector2 position, Color color, float power = 2.5f)
        {
            this.position = position;
            this.color = color;
            this.power = power;
        }
    }

    public List<Key> keys = new List<Key>();

    [HideInInspector] public int version = 0;

    public Gradient2D()
    {
        keys.Add(new Key(new Vector2(0.25f, 0.25f), Color.blue,   2.8f));
        keys.Add(new Key(new Vector2(0.75f, 0.75f), Color.red,    2.8f));
        keys.Add(new Key(new Vector2(0.25f, 0.75f), Color.yellow, 3.5f));
        keys.Add(new Key(new Vector2(0.75f, 0.25f), Color.green,  3.5f));
    }

    public Color Evaluate(Vector2 uv)
    {
        if (keys.Count == 0) return Color.white;
        if (keys.Count == 1) return keys[0].color;

        Color blended = Color.black;
        float totalWeight = 0f;

        foreach (var key in keys)
        {
            float distSq = (key.position - uv).sqrMagnitude;
            if (distSq < 0.00001f) return key.color;

            float weight = 1f / Mathf.Pow(distSq + 0.0001f, key.power);
            blended += key.color * weight;
            totalWeight += weight;
        }

        return totalWeight > 0f ? blended / totalWeight : Color.white;
    }
}