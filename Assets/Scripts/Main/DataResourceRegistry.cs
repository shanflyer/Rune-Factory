using System;
using System.Collections.Generic;
using UnityEngine;

public static class DataResourceRegistry
{
    public static bool TryGetDataPath(Type type, out string path)
    {
        path = DataPath.GetDataPath(type);
        if (!string.IsNullOrEmpty(path))
        {
            return true;
        }

        Debug.LogError($"GameDataManager data path is not registered: {type.FullName}");
        return false;
    }

    public static GameDataManager.DataIntegrityReport ValidateDataPathRegistry()
    {
        var report = new GameDataManager.DataIntegrityReport();
        var pathOwners = new Dictionary<string, Type>();

        foreach (var dataPath in DataPath.dataPathDic)
        {
            report.checkedCount++;
            Type type = dataPath.Key;
            string path = dataPath.Value;
            if (string.IsNullOrEmpty(path))
            {
                report.missingPaths.Add($"{type.FullName}: <empty>");
                continue;
            }

            if (pathOwners.TryGetValue(path, out var ownerType))
            {
                report.duplicatePaths.Add($"{path}: {ownerType.Name}, {type.Name}");
            }
            else
            {
                pathOwners[path] = type;
            }

            UnityEngine.Object singleAsset = ExtensionsResources.LoadResource<UnityEngine.Object>(path);
            UnityEngine.Object[] folderAssets = ExtensionsResources.LoadAllResource<UnityEngine.Object>(path);
            if (singleAsset == null && (folderAssets == null || folderAssets.Length == 0))
            {
                report.missingPaths.Add($"{type.FullName}: {path}");
                continue;
            }

            if (singleAsset != null && !IsCompatibleDataAsset(singleAsset, type))
            {
                report.incompatibleAssets.Add($"{type.FullName}: {path}, asset={singleAsset.GetType().Name}");
            }
        }

        return report;
    }

    public static void LogDataIntegrityReport(GameDataManager.DataIntegrityReport report)
    {
        if (report == null)
        {
            return;
        }

        if (!report.HasProblem)
        {
            Debug.Log($"GameDataManager data integrity report: checked={report.checkedCount}, ok.");
            return;
        }

        Debug.LogWarning($"GameDataManager data integrity report: checked={report.checkedCount}, missing={report.missingPaths.Count}, incompatible={report.incompatibleAssets.Count}, duplicatePath={report.duplicatePaths.Count}.");
        for (int i = 0; i < report.missingPaths.Count; i++)
        {
            Debug.LogWarning($"Missing data path: {report.missingPaths[i]}");
        }
        for (int i = 0; i < report.incompatibleAssets.Count; i++)
        {
            Debug.LogWarning($"Incompatible data asset: {report.incompatibleAssets[i]}");
        }
        for (int i = 0; i < report.duplicatePaths.Count; i++)
        {
            Debug.LogWarning($"Duplicate data path: {report.duplicatePaths[i]}");
        }
    }

    private static bool IsCompatibleDataAsset(UnityEngine.Object asset, Type dataType)
    {
        if (asset == null || dataType == null)
        {
            return false;
        }

        if (dataType.IsInstanceOfType(asset) || asset is TextAsset)
        {
            return true;
        }

        if (asset is IGameData && typeof(IGameData).IsAssignableFrom(dataType))
        {
            return true;
        }

        if (!typeof(IGameData).IsAssignableFrom(dataType))
        {
            // DataPath 里也登记了 GameRandomDataList 这类普通 ScriptableObject，不能套 IDataArray<T> 的 IGameData 约束。
            return false;
        }

        Type dataArrayType = typeof(IDataArray<>).MakeGenericType(dataType);
        return dataArrayType.IsInstanceOfType(asset);
    }
}
