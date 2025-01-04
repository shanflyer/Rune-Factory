using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public delegate void ActionInit(List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false);

public class GameActionDataManager : Singleton<GameActionDataManager>
{
    public Dictionary<string, ActionInit> gameActionDataDelegates = new Dictionary<string, ActionInit>();

    public void GameAction(string typeName, List<Parameter> parameters, int source = 0, int target = 0, int value = -1, SetResult setResult = null, SetValue setValue = null, bool immediately = false)
    {
       //Debug.Log($"GameAction:{typeName}");
        if (gameActionDataDelegates.TryGetValue(typeName, out var actionInit))
        {
            actionInit.Invoke(parameters, source, target, value, setResult, setValue);
        }
        else
        {
            try
            {
                Type type = Type.GetType(typeName);
                var data = Activator.CreateInstance(type);
                MethodInfo meth = type.GetMethod("Init");
                var _Delegate = (ActionInit)meth.CreateDelegate(typeof(ActionInit), data);

                _Delegate.Invoke(parameters, source, target, value, setResult, setValue);
                gameActionDataDelegates.Add(typeName, _Delegate);
            }
            catch(Exception e)
            {
                Debug.LogError($"{typeName}-{e}");
            }
         
        }
    }

    protected override void Clear()
    {
        base.Clear();
    }

    public override void Init()
    {
        base.Init();

        //var baseType = typeof(GameAction);
        // var assembly = typeof(GameAction).Assembly;
        // var types = assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));
    }

    public async void Action(int dataId, SetResult setResult = null, bool immediately = false)
    {
        GameActionData gameActionData = await GameDataManager.instance.GetAsyncData<GameActionData>(dataId);
        if (gameActionData == null)
        {
           // Debug.LogError($"null gameAction:{dataId}");
            return;
        }
        gameActionData.Action(setResult: setResult, immediately: immediately);
    }
}