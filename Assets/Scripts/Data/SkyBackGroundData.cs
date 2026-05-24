#if UNITY_EDITOR
using UnityEditor;
#endif
using Unity.Mathematics;
using UnityEngine;

public class SkyBackGroundData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string dataName;
    public string bgName;
    public Sprite backGround;
    public float speedY;
    public float4 bgOffset;
    public float2 cameraOffset;
    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string sourcePath = "Assets/Texture/Background/";
        backGround = AssetDatabase.LoadAssetAtPath<Sprite>($"{sourcePath}{bgName}.png");
    }
#endif
}
