using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Singleton<T> where T : Singleton<T>
{
    public static T instance
    {
        get
        {
            if (_instance == null&&!SingletonType.Cleared)
            {
                _instance = Activator.CreateInstance<T>();
                _instance.Init();
                if (typeof(T) != typeof(SingletonType))
                { 
                    SingletonType.instance.AddType(_instance.Clear);

                    if (_instance.NeedUpdata)
                    {
                        SingletonType.instance.AddUpDataAction(_instance.UpData);
                    }
                    if (_instance.NeedLateUpdata)
                    {
                        SingletonType.instance.AddUpDataAction(_instance.LateUpData);
                    }
                } 
            }
            return _instance;
        }
    }
    private static T _instance;

    public virtual bool NeedUpdata
    {
        get;
    }
    public virtual bool NeedLateUpdata
    {
        get;
    }
    protected virtual void UpData()
    {

    }
    protected virtual void LateUpData()
    {

    }
    public virtual void Init()
    {

    } 
    protected virtual void Clear()
    { 
        if (NeedUpdata)
        {
            SingletonType.instance.RemoveUpDataAction(_instance.UpData);
        }
        if (NeedLateUpdata)
        {
            SingletonType.instance.RemoveLateUpDataAction(_instance.LateUpData);
        }
        _instance = null;
    } 
}
public delegate void SingletonClear();
public class SingletonType : Singleton<SingletonType>
{
    public HashSet<SingletonClear> TypeClears = new HashSet<SingletonClear>();
    public List<Action> singleUpdatas = new List<Action>();
    public List<Action> singleLateUpdatas = new List<Action>();
    public override void Init()
    {
        base.Init();
        Cleared = false;
    }
    public void AddUpDataAction(Action action)
    {
        if (!singleUpdatas.Contains(action))
        {
            singleUpdatas.Add(action);
        }
    }
    public void RemoveUpDataAction(Action action)
    {
        if (singleUpdatas.Contains(action))
        {
            singleUpdatas.Remove(action);
        }
    }
    public void AddLateUpDataAction(Action action)
    {
        if (!singleLateUpdatas.Contains(action))
        {
            singleLateUpdatas.Add(action);
        }
    }
    public void RemoveLateUpDataAction(Action action)
    {
        if (singleLateUpdatas.Contains(action))
        {
            singleLateUpdatas.Remove(action);
        }
    }
    public void AddType(SingletonClear typeClear)
    { 
        TypeClears.Add(typeClear);
    }
    public static bool Cleared { get; private set; }
    public void ClearAll()
    {
        Cleared = true;
        singleUpdatas.Clear();
        try
        {
            foreach (var typeClear in TypeClears)
            {

                if (typeClear != null)
                {
                    typeClear();
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.ToString());
        }
        TypeClears.Clear();
        Clear();
    }

    public void UpData()
    {
        for(int i = 0; i < singleUpdatas.Count; i++)
        {
            singleUpdatas[i].Invoke();
        }
    }
    public void LateUpData()
    {
        for (int i = 0; i < singleLateUpdatas.Count; i++)
        {
            singleLateUpdatas[i].Invoke();
        }
    }
}