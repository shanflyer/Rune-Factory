using UnityEngine;
#if UNITY_EDITOR
#endif

public class CharacterData : ScriptableObject, IGameData
{
    public string characterName;
    public int id;
    public Gender gender;

    public string iconName;
    public string objName;
    public string headName;


    public SpriteResourceRenference head =>
        AddressableRemoteManager.instance.LoadAddressable<SpriteResourceRenference>(headName);

    public CharacterRuntimeObj obj =>
        AddressableRemoteManager.instance.LoadAddressablePrefab<CharacterRuntimeObj>(objName);
    public Sprite icon => Resources.Load<Sprite>(iconName);
    public int profession;
    public int level;
    public string fightBehavior;
    public int packageId;
    public AttributeType attributeType;
   
   

    public string GetKey()
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return GetKey();
    }

#if UNITY_EDITOR
 
    public void SetReferenceData()
    {
        
    }

#endif
}