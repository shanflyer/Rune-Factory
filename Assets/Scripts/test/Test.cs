using BehaviorDesigner.Runtime;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public struct TestData
{
    public int index;
    public NativeArray<int> datas;
}

public struct TestJob : IJobParallelFor
{
    [NativeDisableUnsafePtrRestriction]
    public unsafe TestData* testData;

    [WriteOnly]
    public NativeArray<int> result;

    public unsafe void Execute(int index)
    {
        result[index] = testData->datas[index];
    }
}

public class Test : MonoBehaviour
{
    public List<int> testData = new List<int>();

    public List<int2> testInt2 = new List<int2>();

    public List<GameTimeKey> gameTimeKeys;
    public List<int2> values;
    public GameTimeKeyInt2DataDictionary gameTimeKeyIntDic;
    public int2 testKey;
    public BehaviorTree behaviorTree;

    public void TestBehavior()
    {
        behaviorTree.OnBehaviorEnd += (Behavior behavior) =>
        {
            Debug.Log($"endtest:{behavior.BehaviorName}");
        };
        behaviorTree.EnableBehavior();
        behaviorTree.OnBehaviorRestart += (Behavior behavior) =>
        {
            Debug.Log($"Restart:{behavior.BehaviorName}");
        };
    }

    public void InitDic()
    {
        gameTimeKeyIntDic = new GameTimeKeyInt2DataDictionary();
        for (int i = 0; i < gameTimeKeys.Count; i++)
        {
            gameTimeKeyIntDic.Add(gameTimeKeys[i], values[i]);
        }
    }

    public void TestDic()
    {
        float nowTime = Time.realtimeSinceStartup;
        if (gameTimeKeyIntDic.TryGetValue(testKey, out var value))
        {
            Debug.Log(Time.realtimeSinceStartup - nowTime);
            Debug.Log($"Value:{value}");
        }
    }

    public unsafe void TestUnsafe()
    {
        TestData Data = new TestData
        {
            datas = new NativeArray<int>(testData.Count, Allocator.TempJob),
        };
        for (int i = 0; i < testData.Count; i++)
        {
            Data.datas[i] = testData[i];
        }
        NativeArray<int> result = new NativeArray<int>(testData.Count, Allocator.TempJob);
        TestJob testJob = new TestJob
        {
            result = result,
            testData = &Data
        };
        testJob.Schedule(testData.Count, 4).Complete();
        for (int i = 0; i < result.Length; i++)
        {
            Debug.Log(result[i]);
        }
        result.Dispose();
        Data.datas.Dispose();
    }

    public List<ItemAnimationData> formulaDatas = new List<ItemAnimationData>();

    public void TestNewtosoftTostring()
    {
        string path = DataPath.GetDataPath(typeof(ItemAnimationData));
        var allData = Resources.LoadAll<ItemAnimationData>(path);
        if (allData != null && allData.Length > 0)
        {
            string strs = JsonConvert.SerializeObject(allData);
            File.WriteAllText("test.json", strs);
            Debug.Log(strs);
        }
    }

    public void TestInt2NewtosoftTostring()
    {
        string strs = JsonConvert.SerializeObject(testInt2, new JsonSerializerSettings()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });
        File.WriteAllText("test.json", strs);
        Debug.Log(strs);
    }

    public void TesttNewtosoftToObj()
    {
        var strs = File.ReadAllText("test.json");
        testInt2 = JsonConvert.DeserializeObject<List<int2>>(strs);
        //formulaDatas = JsonConvert.DeserializeObject<List<ItemAnimationData>>(strs);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }
}

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public Test test => target as Test;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("testBehavior"))
        {
            test.TestBehavior();
        }
        if (GUILayout.Button("≤‚ ‘json–Ú¡–ªØInt2"))
        {
            test.TestInt2NewtosoftTostring();
        }
        if (GUILayout.Button("≤‚ ‘json–Ú¡–ªØ"))
        {
            test.TestNewtosoftTostring();
        }
        if (GUILayout.Button("≤‚ ‘json∑¥–Ú¡–ªØ"))
        {
            test.TesttNewtosoftToObj();
        }
        if (GUILayout.Button("≤‚ ‘job"))
        {
            test.TestUnsafe();
        }
        if (GUILayout.Button("≤‚ ‘Dic"))
        {
            test.InitDic();
            test.TestDic();
        }
    }
}