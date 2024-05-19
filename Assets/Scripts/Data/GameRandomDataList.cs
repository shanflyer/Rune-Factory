using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Burst;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
[CreateAssetMenu(menuName ="Data/随机数据")]
public class GameRandomDataList : ScriptableObject
{
#if UNITY_EDITOR
    [NonSerialized]
    public GameRandomDataEditor[] gameRandomDataEditors;

    public void SetReferenceData()
    {
        gameRandomDatas = new List<GameRandomData>();
        GameRandomData gameRandomData=new GameRandomData();
        foreach (var editorData in gameRandomDataEditors)
        {
            if (editorData.id != gameRandomData.id)
            {
                if (gameRandomData.id != 0)
                {
                    if (gameRandomData.weightRandom)
                    {
                        Pretreatment(ref gameRandomData);
                    }
                   
                    gameRandomDatas.Add(gameRandomData);
                }
                gameRandomData = new GameRandomData 
                {
                    id=editorData.id,
                    text=editorData.text,
                    weightRandom=editorData.weightRandom,
                    randomItems=new List<RandomItem>(),
                    barrels=new List<WeightBarrel>()
                };
            }
            RandomItem randomItem = new RandomItem
            {
                itemId=editorData.itemId,
                isGroup = editorData.isGroup,
                text = editorData.itemText,
                itemValue = editorData.itemValue,
                randomValue = editorData.randomValue,
                maxCount = editorData.maxCount,
                minCount = editorData.minCount
            };
            gameRandomData.randomItems.Add(randomItem);

        }
        if (gameRandomData.id != 0)
        {
            if (gameRandomData.weightRandom)
                Pretreatment(ref gameRandomData);
            gameRandomDatas.Add(gameRandomData);
        }
    }

    public void Pretreatment(ref GameRandomData gameRandomData)
    {
        int totalValue = 0;
        for (int i = 0; i < gameRandomData.randomItems.Count; i++)
        {
            totalValue += gameRandomData.randomItems[i].randomValue;
        }
        int averageValue = (int)(math.ceil((float)totalValue / gameRandomData.randomItems.Count));
        List<Vector2Int> baseRamdomItems = new List<Vector2Int>();
        List<Vector2Int> fillRamdomItems = new List<Vector2Int>();
        for (int i = 0; i < gameRandomData.randomItems.Count; i++)
        {
            if (gameRandomData.randomItems[i].randomValue <= averageValue)
            {
                baseRamdomItems.Add(new Vector2Int(i, gameRandomData.randomItems[i].randomValue));
            }
            else
            {
                fillRamdomItems.Add(new Vector2Int(i, gameRandomData.randomItems[i].randomValue));
            }
        }

        int fillIndex = 0;
        WeightBarrel[] barrels = new WeightBarrel[gameRandomData.randomItems.Count];

        if (fillRamdomItems.Count > 0)
        {
            for (int i = 0; i < barrels.Length; i++)
            {
                if (fillIndex >= fillRamdomItems.Count)
                {
                    WeightBarrel endBarrel = new WeightBarrel
                    {
                        itemIndex = baseRamdomItems[i].x,
                        baseWeight = 10000,
                        fillItemIndex = -1
                    };
                    barrels[i] = endBarrel;
                    break;
                }

                int value = averageValue - baseRamdomItems[i].y;
                int fillValue = fillRamdomItems[fillIndex].y - value;

                int baseWeight = (int)(baseRamdomItems[i].y * 10000 / (float)averageValue);
                WeightBarrel weightBarrel = new WeightBarrel
                {
                    itemIndex = baseRamdomItems[i].x,
                    baseWeight = baseWeight,
                    fillItemIndex = fillRamdomItems[fillIndex].x
                };
                barrels[i] = weightBarrel;

                if (fillValue > averageValue)
                {
                    Vector2Int nowFill = new Vector2Int(fillRamdomItems[fillIndex].x, fillValue);
                    fillRamdomItems[fillIndex] = nowFill;
                }
                else
                {
                    baseRamdomItems.Add(new Vector2Int(fillRamdomItems[fillIndex].x, fillValue));
                    fillIndex++;
                }
            }
        }else
        {
            for (int i = 0; i < barrels.Length; i++)
            {
                WeightBarrel weightBarrel = new WeightBarrel
                {
                    itemIndex = baseRamdomItems[i].x,
                    baseWeight = 10000,
                };
                barrels[i] = weightBarrel;
            }
        }
      

        gameRandomData.barrels = barrels.ToList();

    }
#endif

    public List<GameRandomData> gameRandomDatas = new List<GameRandomData>();
   

    
    public override string ToString()
    {
        return "GameRandomDataList";
    }
     
}

#if UNITY_EDITOR 
public struct GameRandomDataEditor
{
    public string text;
    public int id;
    public bool weightRandom;

    public int itemId;
    public string itemText;
    public bool isGroup;
    public string itemValue;
    public int randomValue;
    public int maxCount;
    public int minCount;

}
#endif


[System.Serializable]
public struct GameRandomData
{
    public string text;
    public int id;
    public bool weightRandom;

    public List<RandomItem> randomItems;

    public List<WeightBarrel> barrels;

    public void Pretreatment()
    {
        int totalValue = 0;
        for (int i = 0; i < randomItems.Count; i++)
        {
            totalValue += randomItems[i].randomValue;
        }
        int averageValue = (int)(math.ceil((float)totalValue /randomItems.Count));
        List<Vector2Int> baseRamdomItems = new List<Vector2Int>();
        List<Vector2Int> fillRamdomItems = new List<Vector2Int>();
        for (int i = 0; i < randomItems.Count; i++)
        {
            if (randomItems[i].randomValue <= averageValue)
            {
                baseRamdomItems.Add(new Vector2Int(i, randomItems[i].randomValue));
            }
            else
            {
                fillRamdomItems.Add(new Vector2Int(i, randomItems[i].randomValue));
            }
        }

        int fillIndex = 0;
        WeightBarrel[] barrels = new WeightBarrel[randomItems.Count];

        if (fillRamdomItems.Count > 0)
        {
            for (int i = 0; i < barrels.Length; i++)
            {
                if (fillRamdomItems.Count > 0 && fillIndex >= fillRamdomItems.Count)
                {
                    WeightBarrel endBarrel = new WeightBarrel
                    {
                        itemIndex = baseRamdomItems[i].x,
                        baseWeight = 10000,
                        fillItemIndex = -1
                    };
                    barrels[i] = endBarrel;
                    break;
                }

                int value = averageValue - baseRamdomItems[i].y;
                int fillValue = -1;
                if (fillRamdomItems.Count > fillIndex)
                {
                    fillValue = fillRamdomItems[fillIndex].y - value;
                }


                int baseWeight = (int)(baseRamdomItems[i].y * 10000 / (float)averageValue);

                WeightBarrel weightBarrel = new WeightBarrel
                {
                    itemIndex = baseRamdomItems[i].x,
                    baseWeight = baseWeight,
                    fillItemIndex = fillRamdomItems.Count > fillIndex ? fillRamdomItems[fillIndex].x : baseRamdomItems[i].x
                };
                barrels[i] = weightBarrel;
                if (baseWeight == 10000)
                {
                    continue;
                }

                if (fillValue > averageValue)
                {
                    Vector2Int nowFill = new Vector2Int(fillRamdomItems[fillIndex].x, fillValue);
                    fillRamdomItems[fillIndex] = nowFill;
                }
                else
                {
                    baseRamdomItems.Add(new Vector2Int(fillRamdomItems[fillIndex].x, fillValue));
                    fillIndex++;
                }
            }

        }
        else
        {
            for (int i = 0; i < barrels.Length; i++)
            {
                WeightBarrel weightBarrel = new WeightBarrel
                {
                    itemIndex = baseRamdomItems[i].x,
                    baseWeight = 10000,
                };
                barrels[i] = weightBarrel;
            }
        }

        this.barrels = barrels.ToList();

    }
}



[System.Serializable]

public struct RandomItem
{ 
    public string text;
    public int itemId;
    public bool isGroup;
    public string itemValue;
    public int randomValue;
    public int maxCount;
    public int minCount;
   // public Vector2Int countRange;
}
[System.Serializable]
[BurstCompile]
public struct WeightBarrel
{
    public int itemIndex;
    public int baseWeight;
    public int fillItemIndex;
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameRandomDataList))]
public class GameRandomDataListEditor : Editor
{
    public GameRandomDataList gameRandomDataList
    {
        get
        {
            return target as GameRandomDataList;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("测试"))
        {
            for(int i=0;i< gameRandomDataList.gameRandomDatas.Count; i++)
            {
                var randomData = gameRandomDataList.gameRandomDatas[i];
                gameRandomDataList.Pretreatment(ref randomData);
                gameRandomDataList.gameRandomDatas[i] = randomData;
            }
            EditorUtility.SetDirty(gameRandomDataList);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif