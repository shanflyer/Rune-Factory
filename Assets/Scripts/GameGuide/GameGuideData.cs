using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/引导数据")]
public class GameGuideData : ScriptableObject,IGameData
{
    public int id;
    public string guideName;
    public List<GuidStepData> guidStepDatas = new List<GuidStepData>();
    public int endAction;

    public bool GetGuidStepData(int stepIndex, out GuidStepData stepData)
    {
        stepData = null;
        if (guidStepDatas == null || stepIndex < 0 || stepIndex >= guidStepDatas.Count)
        {
            return false;
        }

        stepData = guidStepDatas[stepIndex];
        return true;
    }

    public void TriggerEndAction()
    {
        if (endAction != 0)
        {
            GameActionDataManager.instance.Action(endAction);
        }
    }

    public bool IsValidStepIndex(int stepIndex)
    {
        return guidStepDatas != null && stepIndex >= 0 && stepIndex < guidStepDatas.Count;
    }

    public bool HasValidGuideSteps()
    {
        return GetValidationErrors().Count == 0;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        CollectValidationErrors(errors);
        return errors;
    }

    public void CollectValidationErrors(List<string> errors)
    {
        if (errors == null)
        {
            return;
        }

        if (id <= 0)
        {
            errors.Add($"{name}: id must be greater than 0.");
        }

        if (guidStepDatas == null || guidStepDatas.Count == 0)
        {
            errors.Add($"{name}: guide steps are empty.");
            return;
        }

        for (int i = 0; i < guidStepDatas.Count; i++)
        {
            if (guidStepDatas[i] == null)
            {
                errors.Add($"{name}: step {i} is null.");
                continue;
            }

            if (guidStepDatas[i].selectableId <= 0)
            {
                errors.Add($"{name}: step {i} selectableId must be greater than 0.");
            }
        }
    }

    public string GetKey()
    {
        return id.ToString();
    }
    public override string ToString()
    {
        return id.ToString();
    }
    public void SetReferenceData()
    {
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Game Guide/Validate GameGuideData")]
    static void ValidateGameGuideDataAssets()
    {
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameGuideData");
        int errorCount = 0;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
            var data = UnityEditor.AssetDatabase.LoadAssetAtPath<GameGuideData>(path);
            if (data == null)
            {
                continue;
            }

            var errors = data.GetValidationErrors();
            errorCount += errors.Count;
            for (int j = 0; j < errors.Count; j++)
            {
                Debug.LogError($"GameGuideData validation failed: {path}: {errors[j]}", data);
            }
        }

        if (errorCount == 0)
        {
            Debug.Log($"GameGuideData validation passed: checked {guids.Length} assets.");
        }
        else
        {
            Debug.LogError($"GameGuideData validation failed: {errorCount} errors in {guids.Length} assets.");
        }
    }
#endif
}
[Serializable]
public class GuidStepData:IReferenceData
{
    public int selectableId;
    public string showText;
}
