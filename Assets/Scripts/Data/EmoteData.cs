
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string sourcePath = "Assets/Animation/emotes/";
        animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{sourcePath}{animationName}.anim");
    }
#endif
}