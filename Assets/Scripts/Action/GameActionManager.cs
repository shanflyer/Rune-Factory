using System;
using System.Collections.Generic;

public class GameActionManager : Singleton<GameActionManager>
{
    public override bool NeedUpdata { get => true; }

    public delegate void ActionDelegate<T>(T e) where T : GameAction;

    public delegate void ActionBus();

    private Queue<ActionBus> ActionQueue = new Queue<ActionBus>();
    private Dictionary<Type, Delegate> delegates = new Dictionary<Type, Delegate>();
    private HashSet<Delegate> onceDelegates = new HashSet<Delegate>();

    public override void Init()
    {
        base.Init();
        delegates.Clear();
    }

    protected override void Clear()
    {
        base.Clear();
        delegates.Clear();
        ActionQueue.Clear();
    }

    public void AddListener<T>(ActionDelegate<T> del, bool once = false) where T : GameAction
    {
        Type type = typeof(T);
        if (delegates.TryGetValue(type, out Delegate d))
        {
            var _d = d as ActionDelegate<T>;
            _d += del;
            delegates[type] = _d;
        }
        else
        {
            delegates[type] = del;
        }
        if (once)
        {
            onceDelegates.Add(del);
        }
    }

    public void RemoveListener<T>(ActionDelegate<T> del) where T : GameAction
    {
        Type type = typeof(T);
        if (delegates.TryGetValue(type, out Delegate d))
        {
            var _d = d as ActionDelegate<T>;
            _d -= del;
            if (_d == null)
            {
                delegates.Remove(type);
            }
            delegates[type] = _d;
        }
    }

    public void TriggerAction<T>(T gameAction) where T : GameAction
    {
        Type type = typeof(T);
        if (delegates.TryGetValue(type, out Delegate d))
        {
            var _d = d as ActionDelegate<T>;
            if (_d == null)
            {
                delegates.Remove(type);
                return;
            }
            _d(gameAction);

            foreach (Delegate _delegate in d.GetInvocationList())
            {
                if (onceDelegates.Contains(_delegate))
                {
                    _d -= _delegate as ActionDelegate<T>;
                    if (_d == null)
                    {
                        delegates.Remove(type);
                    }
                    onceDelegates.Remove(_delegate);
                }
            }
        }
    }

    public void QueueAction<T>(T gameAction, bool immediately = false) where T : GameAction
    {
        if (immediately)
        {
            TriggerAction(gameAction);
        }
        else
        {
            Type type = typeof(T);
            if (delegates.ContainsKey(type))
            {
                ActionQueue.Enqueue(() =>
                {
                    TriggerAction(gameAction);
                });
            }
        }
    }

    protected override void UpData()
    {
        while (ActionQueue.Count > 0)
        { 
            var gameAction = ActionQueue.Dequeue();
            if (gameAction != null)
            {
                gameAction();
            }
            
        }
    }
}