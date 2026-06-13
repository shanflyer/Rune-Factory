using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public static class SingletonPlayerLoopDriver
{
    private struct SingletonUpdateLoop
    {
    }

    private struct SingletonFixedUpdateLoop
    {
    }

    private struct SingletonLateUpdateLoop
    {
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
        var loop = PlayerLoop.GetCurrentPlayerLoop();

        InsertBefore<Update.ScriptRunBehaviourUpdate>(
            ref loop,
            typeof(SingletonUpdateLoop),
            UpdateSingletons);

        InsertBefore<FixedUpdate.ScriptRunBehaviourFixedUpdate>(
            ref loop,
            typeof(SingletonFixedUpdateLoop),
            FixedUpdateSingletons);

        InsertBefore<PreLateUpdate.ScriptRunBehaviourLateUpdate>(
            ref loop,
            typeof(SingletonLateUpdateLoop),
            LateUpdateSingletons);

        PlayerLoop.SetPlayerLoop(loop);
    }

    private static void UpdateSingletons()
    {
        if (Application.isPlaying && !SingletonType.Cleared)
        {
            SingletonType.instance.Update();
        }
    }

    private static void FixedUpdateSingletons()
    {
        if (Application.isPlaying && !SingletonType.Cleared && SingletonType.HasInstance)
        {
            SingletonType.instance.FixedUpdate();
        }
    }

    private static void LateUpdateSingletons()
    {
        if (Application.isPlaying && !SingletonType.Cleared && SingletonType.HasInstance)
        {
            SingletonType.instance.LateUpdate();
        }
    }

    private static bool InsertBefore<TTarget>(ref PlayerLoopSystem root, Type driverType, PlayerLoopSystem.UpdateFunction updateFunction)
    {
        if (Contains(root, driverType))
        {
            return false;
        }

        if (root.subSystemList == null)
        {
            return false;
        }

        var targetType = typeof(TTarget);
        for (int i = 0; i < root.subSystemList.Length; i++)
        {
            if (root.subSystemList[i].type == targetType)
            {
                var systems = root.subSystemList;
                Array.Resize(ref systems, systems.Length + 1);
                Array.Copy(systems, i, systems, i + 1, systems.Length - i - 1);
                systems[i] = new PlayerLoopSystem
                {
                    type = driverType,
                    updateDelegate = updateFunction
                };
                root.subSystemList = systems;
                return true;
            }

            var subSystem = root.subSystemList[i];
            if (InsertBefore<TTarget>(ref subSystem, driverType, updateFunction))
            {
                root.subSystemList[i] = subSystem;
                return true;
            }
        }

        return false;
    }

    private static bool Contains(PlayerLoopSystem root, Type driverType)
    {
        if (root.type == driverType)
        {
            return true;
        }

        if (root.subSystemList == null)
        {
            return false;
        }

        for (int i = 0; i < root.subSystemList.Length; i++)
        {
            if (Contains(root.subSystemList[i], driverType))
            {
                return true;
            }
        }

        return false;
    }
}
