using BehaviorDesigner.Runtime;
using UnityEngine;

public enum BehaviorRunnerType
{
    Event,
    Character,
    MultiNPC
}

public static class BehaviorVariableNames
{
    public const string EventId = "ID";
    public const string CharacterId = "CharacterId";
    public const string FightCharacter = "fightCharacter";
    public const string GroupCharacters = "GroupCharacters";
    public const string GroupId = "GroupID";
}

public sealed class BehaviorTreeRunner
{
    public int ownerId;
    public string ownerName;
    public BehaviorRunnerType runnerType;
    public GameObject host;
    public BehaviorTree tree;
    public bool bound;

    public static BehaviorTreeRunner Create(GameObject parent, BehaviorRunnerType runnerType, int ownerId, string ownerName)
    {
        var root = GetOrCreateRoot(parent, runnerType);
        var safeName = string.IsNullOrEmpty(ownerName) ? "Unnamed" : ownerName;
        var host = new GameObject($"{runnerType}_{ownerId}_{safeName}");
        host.transform.SetParent(root.transform, false);

        return new BehaviorTreeRunner
        {
            ownerId = ownerId,
            ownerName = safeName,
            runnerType = runnerType,
            host = host,
            tree = host.AddComponent<BehaviorTree>()
        };
    }

    public void Rename(string newOwnerName)
    {
        ownerName = string.IsNullOrEmpty(newOwnerName) ? ownerName : newOwnerName;
        if (host != null)
        {
            host.name = $"{runnerType}_{ownerId}_{ownerName}";
        }
    }

    public void Destroy()
    {
        if (tree != null)
        {
            tree.StopAllTaskCoroutines();
            tree.DisableBehavior();
        }

        if (host != null)
        {
            Object.Destroy(host);
        }
    }

    private static GameObject GetOrCreateRoot(GameObject parent, BehaviorRunnerType runnerType)
    {
        var rootName = $"{runnerType}Runners";
        var parentTransform = parent != null ? parent.transform : null;
        if (parentTransform != null)
        {
            var existing = parentTransform.Find(rootName);
            if (existing != null)
            {
                return existing.gameObject;
            }
        }

        var root = new GameObject(rootName);
        if (parentTransform != null)
        {
            root.transform.SetParent(parentTransform, false);
        }
        return root;
    }
}
