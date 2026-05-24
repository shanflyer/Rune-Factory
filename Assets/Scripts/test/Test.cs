using BehaviorDesigner.Runtime;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using System;
#if UNITY_EDITOR
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
[Serializable]
public class Int2Test
{
    public Int2IntDictionary dic;
}
public struct TestStruct
{
    public List<int> list;
}
public class Test : MonoBehaviour
{
    [SerializeField]
    private string jsonStr;
    [SerializeField]
    private  List<int> playerPackages;
    [SerializeField]
    UserGameSaveData UserGameSaveData;
 
    public  List<int3> datas = new List<int3>();

    public void TestInt2Dictionary()
    {
        Int2IntDictionary int2Dic = new Int2IntDictionary();
        for(int i = 0; i < datas.Count; i++)
        {
            int2Dic.Add(datas[i].xy, datas[i].z);
        }
        Int2Test int2Test = new Int2Test
        {
            dic = int2Dic
        };

        Type type = typeof(Int2Test);
        var files = type.GetFields();

        Int2Test int2Test1 = new Int2Test();

        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var value = file.GetValue(int2Test);
            var str = JsonConvert.SerializeObject(value);
            var obj = JsonConvert.DeserializeObject(str, file.FieldType);
            file.SetValue(int2Test1, obj);
        }
         
      
    }
    public void TestUserGameSaveData()
    {
        Type type = typeof(UserGameSaveData);
        var fields= type.GetFields();

        for(int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            if (field.FieldType == typeof(string))
            {
                Debug.Log($"string--{field.Name}");
            }else if (field.FieldType == typeof(int))
            {
                Debug.Log($"int--{field.Name}");
            }
            else
            {
                Debug.Log($"other--{field.Name}");
            }

        }
    }

    public void TestObjToJson()
    {
        jsonStr = JsonConvert.SerializeObject(playerPackages);
    }
    public void TestJsonToObj()
    {
        playerPackages=JsonConvert.DeserializeObject<List<int>>(jsonStr);
    }


    public GameObject prefab;
    public GameObject obj;
    public void TestCreatObj()
    {
        AsyncTaskRunner.Run(TestCreatObjAsync, nameof(TestCreatObj));
    }

    public async System.Threading.Tasks.Task TestCreatObjAsync()
    {
        var AsyncInstantiateOperation = InstantiateAsync(prefab);
        await AsyncInstantiateOperation;
        obj = AsyncInstantiateOperation.Result[0];
    }

    public TestStruct testStruct;

    public void TestStructAction()
    {
        List<TestStruct> testStructs = new List<TestStruct>();
        testStruct = new TestStruct
        {
            list = new List<int>
            {
                1,2,3,4
            }
        };
        testStructs.Add(testStruct);
        string log = "zero:";
        for (int i = 0; i < testStruct.list.Count; i++)
        {
            log += (testStruct.list[i]) + ",";
        }
        Debug.Log(log);

        var t = testStructs[0];
        t.list.Add(999);
        log = "add:";
        for (int i = 0; i < testStructs[0].list.Count; i++)
        {
            log += (testStructs[0].list[i]) + ",";
        }
        Debug.Log(log);
        var t1 = testStructs[0];
        t1.list.RemoveAt(2);
        log = "remove:";
        for (int i = 0; i < testStructs[0].list.Count; i++)
        {
            log += (testStructs[0].list[i]) + ",";
        }
        Debug.Log(log);
    }


    public List<int> testData = new List<int>();

    public List<int2> testInt2 = new List<int2>();

    public List<GameTimeKey> gameTimeKeys;
    public List<int2> values;
    public GameTimeKeyInt2DataDictionary gameTimeKeyIntDic;
    public int2 testKey;
    public BehaviorTree behaviorTree;

    public RectTransform transform0;
    public RectTransform transform1;

    public void TestTransform()
    {
        transform1.position = transform0.position;
        transform1.sizeDelta = transform0.sizeDelta;
    }

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
    public void DisEnableBehavior()
    {
        behaviorTree.DisableBehavior();
    }
    public void StopEnableBehavior()
    {
        behaviorTree.StopAllTaskCoroutines();
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
        if (GUILayout.Button("TestType"))
        {
            test.TestUserGameSaveData();
        }
        if (GUILayout.Button("estInt2Dictionary"))
        {
            test.TestInt2Dictionary();
        }

        if (GUILayout.Button("testCreat"))
        {
            test.TestCreatObj();
        }
        if (GUILayout.Button("TestTransform"))
        {
            test.TestTransform();
        }
    }
}
#endif


