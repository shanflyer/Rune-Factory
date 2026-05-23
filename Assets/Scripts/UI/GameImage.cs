using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class GameImage : Image
{
    [SerializeField] bool m_GameNullClear;
    [SerializeField] Color m_GameColorTL = Color.white;
    [SerializeField] Color m_GameColorTR = Color.white;
    [SerializeField] Color m_GameColorBL = Color.white;
    [SerializeField] Color m_GameColorBR = Color.white;
    [SerializeField] bool m_GameColorGradient;

    public bool NullClear
    {
        get => m_GameNullClear;
        set
        {
            if (m_GameNullClear == value)
                return;

            m_GameNullClear = value;
            SetVerticesDirty();
        }
    }

    public Color colorTL
    {
        get => m_GameColorTL;
        set => SetGradientColor(ref m_GameColorTL, value);
    }

    public Color colorTR
    {
        get => m_GameColorTR;
        set => SetGradientColor(ref m_GameColorTR, value);
    }

    public Color colorBL
    {
        get => m_GameColorBL;
        set => SetGradientColor(ref m_GameColorBL, value);
    }

    public Color colorBR
    {
        get => m_GameColorBR;
        set => SetGradientColor(ref m_GameColorBR, value);
    }

    public bool colorGradient
    {
        get => m_GameColorGradient;
        set
        {
            if (m_GameColorGradient == value)
                return;

            m_GameColorGradient = value;
            SetVerticesDirty();
        }
    }

    public override void SetNativeSize()
    {
        Sprite active = overrideSprite != null ? overrideSprite : sprite;
        if (active != null)
        {
            base.SetNativeSize();
            return;
        }

        rectTransform.sizeDelta = Vector2.zero;
        SetAllDirty();
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        Sprite active = overrideSprite != null ? overrideSprite : sprite;
        if (active == null && m_GameNullClear)
        {
            toFill.Clear();
            return;
        }

        base.OnPopulateMesh(toFill);
        ApplyGradient(toFill);
    }

    void SetGradientColor(ref Color target, Color value)
    {
        if (target == value)
            return;

        target = value;
        m_GameColorGradient = true;
        SetVerticesDirty();
    }

    void ApplyGradient(VertexHelper vh)
    {
        if (!m_GameColorGradient || vh.currentVertCount == 0)
            return;

        List<UIVertex> vertices = ListPool<UIVertex>.Get();
        vh.GetUIVertexStream(vertices);

        Vector2 min = vertices[0].position;
        Vector2 max = vertices[0].position;
        for (int i = 1; i < vertices.Count; i++)
        {
            Vector3 position = vertices[i].position;
            min = Vector2.Min(min, position);
            max = Vector2.Max(max, position);
        }

        float width = Mathf.Max(max.x - min.x, Mathf.Epsilon);
        float height = Mathf.Max(max.y - min.y, Mathf.Epsilon);
        for (int i = 0; i < vertices.Count; i++)
        {
            UIVertex vertex = vertices[i];
            float x = Mathf.InverseLerp(min.x, min.x + width, vertex.position.x);
            float y = Mathf.InverseLerp(min.y, min.y + height, vertex.position.y);
            Color bottom = Color.Lerp(m_GameColorBL, m_GameColorBR, x);
            Color top = Color.Lerp(m_GameColorTL, m_GameColorTR, x);
            vertex.color = Color.Lerp(bottom, top, y);
            vertices[i] = vertex;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(vertices);
        ListPool<UIVertex>.Release(vertices);
    }
}
