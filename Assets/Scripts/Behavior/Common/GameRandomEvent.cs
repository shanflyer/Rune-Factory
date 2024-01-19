using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

[TaskCategory("NewGame/Common")]
[TaskName("Ëæ»ú½á¹û")]
public class GameRandomEvent : Action
{
    public SharedIntList randomSources;
    public SharedInt result;
    public SharedIntList withOutSource;

    public SharedInt randomId;
    public SharedRandomResults randomResults;

    public override void OnStart()
    {
        if (randomSources != null && randomSources.Value != null)
        {
            List<int> sources = new List<int>();
            sources.AddRange(randomSources.Value);
            List<int> withOuts = null;
            if (withOutSource != null)
            {
                withOuts = withOutSource.Value;
            }
            if (withOuts != null)
            {
                for (int i = 0; i < withOuts.Count; i++)
                {
                    sources.Remove(withOuts[i]);
                }
            }
            int index = GameRandom.RandomInt(0, sources.Count);
            result.Value = sources[index];
        }
        else
        {
            var results = GameRandom.instance.GetRandomValue(randomId.Value);
            randomResults.Value = results;
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}