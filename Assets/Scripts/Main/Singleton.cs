using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class Singleton<T> where T : Singleton<T>
{
    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Activator.CreateInstance<T>();
                _instance.Init();
                if (typeof(T) != typeof(SigletonType))
                {
                    SigletonType.instance.AddType(_instance.Clear);
                }
            }
            return _instance;
        }
    }
    private static T _instance;

  

    public virtual void Init()
    {

    }
     
    protected virtual void Clear()
    {
        _instance = null;
    }
}
public delegate void SingletonClear();
public class SigletonType : Singleton<SigletonType>
{
    public HashSet<SingletonClear> TypeClears = new HashSet<SingletonClear>();

    public void AddType(SingletonClear typeClear)
    { 
        TypeClears.Add(typeClear);
    }
    public void ClearAll()
    {
        foreach(var typeClear in TypeClears)
        {
             
            if (typeClear != null)
            {
                typeClear();
            }
        }
        Clear();
    }
}