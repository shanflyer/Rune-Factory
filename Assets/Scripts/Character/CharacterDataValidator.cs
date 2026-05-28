#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CharacterDataValidator
{
    [MenuItem("Tools/Character/Validate Character Data")]
    public static void ValidateCharacterData()
    {
        var warnings = CollectWarnings();

        if (warnings.Count == 0)
        {
            Debug.Log("CharacterDataValidator completed with 0 warnings.");
            return;
        }

        Debug.LogWarning($"CharacterDataValidator completed with {warnings.Count} warnings.\n{string.Join("\n", warnings)}");
    }

    public static List<string> CollectWarnings()
    {
        var warnings = new List<string>();
        var npcDataById = LoadAssetMap<NPCData>();

        ValidateAssets<CharacterData>(warnings, ValidateCharacterDataAsset);
        ValidateAssets<NPCData>(warnings, ValidateNpcDataAsset);
        ValidateAssets<NPCBehaviorData>(warnings,
            (data, path, list) => ValidateNpcBehaviorDataAsset(data, path, list, npcDataById));
        ValidateAssets<NPCTaskScheduleData>(warnings, ValidateNpcTaskScheduleDataAsset);
        ValidateAssets<TaskScheduleModelDataList>(warnings, ValidateTaskScheduleModelDataListAsset);

        return warnings;
    }

    private static Dictionary<int, NPCData> LoadAssetMap<T>() where T : Object, IGameData
    {
        var map = new Dictionary<int, NPCData>();
        foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.LoadAssetAtPath<T>(path) is NPCData npcData)
            {
                map[npcData.id] = npcData;
            }
        }

        return map;
    }

    private static void ValidateAssets<T>(List<string> warnings, System.Action<T, string, List<string>> validate)
        where T : Object
    {
        foreach (var guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                warnings.Add($"{typeof(T).Name}: failed to load asset at {path}.");
                continue;
            }

            validate(asset, path, warnings);
        }
    }

    private static void ValidateCharacterDataAsset(CharacterData data, string path, List<string> warnings)
    {
        WarnIf(data.id <= 0, warnings, path, "id must be positive.");
        WarnIf(string.IsNullOrEmpty(data.characterName), warnings, path, "characterName is empty.");
        WarnIf(data.profession <= 0, warnings, path, "profession is missing.");
        WarnIf(data.level < 0, warnings, path, "level is negative.");
        WarnIf(data.packageId < 0, warnings, path, "packageId is negative.");
        WarnIf(string.IsNullOrEmpty(data.objName), warnings, path, "objName is empty.");
    }

    private static void ValidateNpcDataAsset(NPCData data, string path, List<string> warnings)
    {
        WarnIf(data.id <= 0, warnings, path, "id must be positive.");
        WarnIf(data.linkCharacterId <= 0, warnings, path, "linkCharacterId is missing.");
        WarnIf(data.functionIds != null && data.friendLevels != null && data.functionIds.Count != data.friendLevels.Count,
            warnings, path, "functionIds and friendLevels count mismatch.");
    }

    private static void ValidateNpcBehaviorDataAsset(NPCBehaviorData data, string path, List<string> warnings,
        Dictionary<int, NPCData> npcDataById)
    {
        WarnIf(data.id <= 0, warnings, path, "id must be positive.");
        bool hasNoSchedule = data.dailyTasks == null || data.dailyTasks.Count == 0;
        bool isHiddenStaticNpc = npcDataById.TryGetValue(data.id, out var npcData) && npcData.hide && hasNoSchedule;

        WarnIf(hasNoSchedule && !isHiddenStaticNpc, warnings, path, "dailyTasks is empty.");
        WarnIf(data.externalBehavior == null && !isHiddenStaticNpc, warnings, path, "externalBehavior is missing.");
    }

    private static void ValidateNpcTaskScheduleDataAsset(NPCTaskScheduleData data, string path, List<string> warnings)
    {
        WarnIf(data.id <= 0, warnings, path, "id must be positive.");
        WarnIf(string.IsNullOrEmpty(data.taskName), warnings, path, "taskName is empty.");
        WarnIf(data.externalBehavior == null, warnings, path, "externalBehavior is missing.");
        WarnIf(data.maxPauseTime < 0, warnings, path, "maxPauseTime is negative.");
    }

    private static void ValidateTaskScheduleModelDataListAsset(TaskScheduleModelDataList data, string path,
        List<string> warnings)
    {
        if (data.taskScheduleModelDatas == null)
        {
            warnings.Add($"{path}: taskScheduleModelDatas is null.");
            return;
        }

        foreach (var schedule in data.taskScheduleModelDatas)
        {
            if (schedule == null)
            {
                warnings.Add($"{path}: contains null TaskScheduleModelData.");
                continue;
            }

            WarnIf(schedule.id <= 0, warnings, path, $"schedule {schedule.modelName} id must be positive.");
            WarnIf(!schedule.gameTimeKey.HasValidRange(), warnings, path,
                $"schedule {schedule.id} has invalid time range {schedule.gameTimeKey}.");
            WarnIf(schedule.dailyTaskDataItems == null || schedule.dailyTaskDataItems.Count == 0,
                warnings, path, $"schedule {schedule.id} dailyTaskDataItems is empty.");
        }
    }

    private static void WarnIf(bool condition, List<string> warnings, string path, string message)
    {
        if (condition)
        {
            warnings.Add($"{path}: {message}");
        }
    }
}
#endif
