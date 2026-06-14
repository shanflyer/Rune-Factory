using System;
using System.Collections.Generic;
using System.Linq;
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
             (actionInit.Target as GameAction).Clear();
            actionInit.Invoke(parameters, source, target, value, setResult, setValue);
        }
        else
        {
            try
            {
                Type type = Type.GetType(typeName);
                var data = Activator.CreateInstance(type);
                MethodInfo meth = type.GetMethod("Init");
                if (meth == null)
                {
                    var interfaces = type.GetInterfaces().Except(type.BaseType?.GetInterfaces() ?? Type.EmptyTypes)
                        .ToArray();

                    foreach (var face in interfaces)
                    {
                        meth = face.GetMethod("Init");
                        if (meth != null) break;
                    }
                }
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

    public void Action(
        int dataId,
        int source = 0,
        int target = 0,
        int value = -1,
        SetResult setResult = null,
        SetValue setValue = null,
        bool immediately = false)
    {
        AsyncTaskRunner.Run(() => ActionAsync(dataId, source, target, value, setResult, setValue, immediately), nameof(GameActionDataManager.Action));
    }

    private async System.Threading.Tasks.Task ActionAsync(
        int dataId,
        int source = 0,
        int target = 0,
        int value = -1,
        SetResult setResult = null,
        SetValue setValue = null,
        bool immediately = false)
    {
        var gameActionData = await GameDataManager.instance.GetAsyncData<GameActionAsset>(dataId);
        if (gameActionData == null)
        {
            return;
        }

        gameActionData.Action(source, target, value, setResult, setValue, immediately);
    }
}
