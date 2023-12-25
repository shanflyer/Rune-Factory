using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SpriteRenderer))]
public class SpriteRendererEditor : Editor
{
    private SpriteRenderer spriteRenderer =>target as SpriteRenderer;
    static string[] layerNames;
    static Dictionary<string, int> layerIndexs = new Dictionary<string, int>();
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        try
        {
            if (layerNames == null || layerNames.Length != SortingLayer.layers.Length)
            {
                layerNames = new string[SortingLayer.layers.Length];
                for (int i = 0; i < SortingLayer.layers.Length; i++)
                {
                    layerNames[i] = SortingLayer.layers[i].name;
                    layerIndexs.Add(layerNames[i], i);
                }

            }

            int layerValue = layerIndexs[spriteRenderer.sortingLayerName];
            layerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, layerNames);

            SortingLayer layer = SortingLayer.layers[layerValue];
            spriteRenderer.sortingLayerName = layer.name;
            spriteRenderer.sortingLayerID = layer.id;
            spriteRenderer.sortingOrder = EditorGUILayout.IntField("Order in Layer", spriteRenderer.sortingOrder);

            Color color = spriteRenderer.color;
            color = EditorGUILayout.ColorField(new GUIContent("OverrideColor"), color, false, true, true);
            spriteRenderer.color = color;
        }
        catch { }

        /*
        InitSpriteTrueUV initSpriteTrueUV;
        if(!meshRenderer.transform.TryGetComponent<InitSpriteTrueUV>(out initSpriteTrueUV))
        {
            meshRenderer.gameObject.AddComponent<InitSpriteTrueUV>();
        }*/
    }
}