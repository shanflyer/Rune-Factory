using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Game/Character")]
[TaskName("ÉèÖÃ½ÇÉ«¶¯»­")]

public class SetCharacterAnimation : Action
{
    private SharedInt characterId;
    public AnimationParameter[] animationParameters;
    public override void OnStart()
    {
        if (characterId==null|| characterId.IsNull())
        {
            characterId = (SharedInt)Owner.GetVariable("CharacterId");
        }
        if (animationParameters != null && animationParameters.Length > 0)
        {
            for (int i = 0; i < animationParameters.Length; i++)
            {
                var animationParameter = animationParameters[i];

                SetCharacterAnimator setCharacterAnimator = new SetCharacterAnimator
                {
                    characterId = characterId.Value,
                    parameter = animationParameter.parameter,
                    parameterType = animationParameter.parameterType,
                    boolValue = animationParameter.boolValue.Value,
                    intValue = animationParameter.intValue.Value,
                    floatValue = animationParameter.floatValue.Value
                };
                GameActionManager.instance.QueueAction(setCharacterAnimator, true);
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
       
        return TaskStatus.Success;
    }
}
public enum ParameterType
{
    BOOL, INT, FLOAT,TRIGGER
}
[System.Serializable]
public struct AnimationParameter
{
    public string parameter;
    public ParameterType parameterType;
    public SharedBool boolValue;
    public SharedInt intValue;
    public SharedFloat floatValue;
}