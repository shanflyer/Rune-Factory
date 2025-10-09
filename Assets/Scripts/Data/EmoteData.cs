using UnityEngine;
#if UNITY_EDITOR
#endif
public class EmoteData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string emoteName;
    public string animationName;

    public AnimationClip animationClip
    {
        get
        {
            var sourcePath = "Animation/emotes/";
            return Resources.Load<AnimationClip>($"{sourcePath}{animationName}");
        }
    }
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
        
    }
#endif
}