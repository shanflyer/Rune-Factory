using UnityEngine;
#if UNITY_EDITOR
#endif

public class NPCFunctionData : ScriptableObject, IGameData,IReferenceData
{
    public int id;
    public string npcFunctionName;
    public string iconName;
    public Sprite icon;
    public int closeTalk;
    public int checkAction;
    [Header("交互事件")]
    public int OperateAction;
    [Header("显示时Action")]
    public GameActionAsset GameActionData;
    public string GetKey()
    {
        return id.ToString();
    }
    public string GetName()
    {
        return npcFunctionName;
    }
    public override string ToString()
    {
        return id.ToString();
    }
#if UNITY_EDITOR
    public void SetReferenceData()
    {
        string path = $"Reference/{iconName}";
       var spriteRenference= ExtensionsResources.LoadResource<SpriteResourceRenference>(path);
        icon = spriteRenference.sprite;
    }
#endif

}
