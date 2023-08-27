
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using UnityEngine;

public delegate void ActionInit(List<Parameter> parameters);
public class GameActionDataManager : Singleton<GameActionDataManager>
{
   public  Dictionary<string, ActionInit> gameActionDataDelegates = new Dictionary<string, ActionInit>();

    public void GameAction(string typeName, List<Parameter> parameters)
    {
        if(gameActionDataDelegates.TryGetValue(typeName,out var actionInit))
        {
            actionInit.Invoke(parameters);
        }
        else
        {
            Type type = Type.GetType(typeName);
            var data = Activator.CreateInstance(type);
            MethodInfo meth = type.GetMethod("Init");
            var _Delegate =(ActionInit) meth.CreateDelegate(typeof(ActionInit), data);
           
             _Delegate.Invoke(parameters);
             gameActionDataDelegates.Add(typeName, _Delegate);
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



}