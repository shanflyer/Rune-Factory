using System;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/角色动画状态包装")]
public class CharacterAnimatorStateParameterDataList : ScriptableObject, IGameData, IDataArray<CharacterAnimatorStateParameterData>
{
    private CharacterAnimatorStateParameterData[] characterAnimatorStateParameterDatas;
    public CharacterAnimatorStateParameterData[] DataList => throw new NotImplementedException();

    public string GetKey()
    {
        return "CharacterAnimatorStateParameterDataList";
    }

    public void SetReferenceData()
    {
    }

}

public struct CharacterAnimatorStateParameterData : IGameData
{
    public string animatorStateName;
    public string parameterName;
    public ParameterType parameterType;

    public string GetKey()
    {
        return animatorStateName;
    }
    public override string ToString()
    {
        return animatorStateName;
    }
    public void SetReferenceData()
    {
    }
}
