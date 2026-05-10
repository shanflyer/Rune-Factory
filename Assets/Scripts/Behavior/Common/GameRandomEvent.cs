using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

public enum RandomType
{
    默认,随机区间
}

[TaskCategory("NewGame/Common")]
[TaskName("随机结果")]
public class GameRandomEvent : Action
{
    public RandomType randomType;
    public SharedIntList randomSources;
    public SharedInt result;
    public SharedIntList withOutSource;

    public SharedInt randomId;
    public SharedRandomResults randomResults;

    public override void OnStart()
    {
        if (randomSources != null && randomSources.Value != null&& randomSources.Value.Count>0)
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
            if (sources.Count == 0)
            {
                sources.AddRange(randomSources.Value);
            }
            int index = GameRandom.RandomInt(0, sources.Count);
            result.Value = sources[index];
        }
        else
        {
            var results = GameRandom.instance.GetRandomValue(randomId.Value);
            randomResults.Value = results;
            if (results.Count > 0)
            {
                result.SetValue(results[0].x);
            }
        }
    }

    public override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}