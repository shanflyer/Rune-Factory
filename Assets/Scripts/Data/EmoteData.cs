using UnityEditor;
using UnityEngine;

public class EmoteData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string emoteName;
    public string animationName;
    public AnimationClip animationClip;
    public int X, Y;

    public override string ToString()
    {
        return id.ToString();
    }
    public string GetKey()
    {
        return id.ToString();
    }

    public void SetReferenceData()
    {
        string sourcePath = "Assets/Animation/emotes/";
        animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{sourcePath}{animationName}.anim");
    }
}