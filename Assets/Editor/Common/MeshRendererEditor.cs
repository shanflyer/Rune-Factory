using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererEditor : Editor
{
    private MeshRenderer meshRenderer;

    static string[] layerNames;
    static Dictionary<string, int> layerIndexs = new Dictionary<string, int>();
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        try
        {
            meshRenderer = target as MeshRenderer;
            if (layerNames==null||layerNames.Length != SortingLayer.layers.Length)
            {
                layerNames = new string[SortingLayer.layers.Length];
                for (int i = 0; i < SortingLayer.layers.Length; i++)
                {
                    layerNames[i] = SortingLayer.layers[i].name;
                    layerIndexs.Add(layerNames[i], i);
                }
                   
            }
           

            int layerValue = layerIndexs[meshRenderer.sortingLayerName];
            layerValue = EditorGUILayout.Popup("Sorting Layer", layerValue, layerNames);

            SortingLayer layer = SortingLayer.layers[layerValue];
            meshRenderer.sortingLayerName = layer.name;
            meshRenderer.sortingLayerID = layer.id;
            meshRenderer.sortingOrder = EditorGUILayout.IntField("Order in Layer", meshRenderer.sortingOrder);
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