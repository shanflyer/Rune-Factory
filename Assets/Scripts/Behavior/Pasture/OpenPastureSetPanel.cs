using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;
using UnityEngine;

[TaskCategory("Game/牧场")]
[TaskName("打开牧场分配界面")]
public class OpenPastureSetPanel : Action
{
    [SerializeField]
    private SharedBool showTeam;
    [SerializeField]
    private SharedIntList otherAnimals;
    public override void OnStart()
    {
        AsyncTaskRunner.Run(OnStartAsync, nameof(OpenPastureSetPanel));
    }

    private async System.Threading.Tasks.Task OnStartAsync()
    {

        MyListInt myListInt = new MyListInt();
        List<int> ints = new List<int>();
        if (otherAnimals != null && otherAnimals.Value != null)
        {
            var animals = otherAnimals.Value;
            for(int i = 0; i < animals.Count; i++)
            {
                ints.Add(animals[i]);
            }
        }
        if (showTeam.Value)
        {
            Team team = TeamManager.instance.playerTeam;
            var teamers= team.Teamers;
            for(int i = 0; i < teamers.Count; i++)
            {
                ints.Add(teamers[i].character.instanceId);
            }
        }
        myListInt.intList = ints;
       await UIManager.instance.ShowGamePanel<PasturePanel, MyListInt>(myListInt);

        taskStatus = TaskStatus.Success;
    }

    private TaskStatus taskStatus;

    public override TaskStatus OnUpdate()
    {
        return taskStatus;
    }
}
