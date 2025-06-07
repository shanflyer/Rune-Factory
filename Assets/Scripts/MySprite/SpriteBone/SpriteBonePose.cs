using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteBonePose", menuName = "Scriptable Objects/SpriteBonePose")]
public class SpriteBonePose : ScriptableObject
{
    public List<BonePose> DisplayBonePoses = new List<BonePose>();
    public List<LayerPart> LayerParts = new List<LayerPart>();
    public List<string> HideGroup = new List<string>();
    public string DisplayGroup;

    public void Clear()
    {
        DisplayBonePoses.Clear();
        LayerParts.Clear();
        HideGroup.Clear();
    }
}
[Serializable]
public struct LayerPart
{
    public string name;
    public int layerOrder;
    public LayerPart(SpriteRenderer spriteRenderer, string name)
    {
        this.name = name;
        layerOrder = spriteRenderer.sortingOrder;
    }
}
[Serializable]
public struct BonePose
{
    public string name;
    public Vector3 position;
    public Vector3 scale;
    public Vector3 angle;
    public BonePose(Transform child,string name)
    {
        this.name = name;
        position = child.localPosition;
        scale = child.localScale;
        angle = child.localEulerAngles;
    }
}