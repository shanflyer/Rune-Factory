using System.Collections;
using UnityEngine;

public class MoneyCreatData : ScriptableObject, IGameData, IReferenceData
{
    public int id;
    public string creatName;
    public string IconName;
    public SpriteResourceRenference Icon;
    public PayType getPayType;
    public int getValue;
    public PayType costPayType;
    public int costValue;
    public string notice;
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
        Icon = Resources.Load<SpriteResourceRenference>($"Reference/{IconName}");
    }
} 