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
    private GameRandomDataEditor[] gameRandomDataEditors;

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
                    barrels=new List<int3>()
                };
            }
            RandomItem randomItem = new RandomItem
            { 
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
        int3[] barrels = new int3[gameRandomData.randomItems.Count];

        if (fillRamdomItems.Count > 0)
        {
            for (int i = 0; i < barrels.Length; i++)
            {
                if (fillIndex >= fillRamdomItems.Count)
                {
                    int3 endBarrel = new int3(baseRamdomItems[i].x, 10000, -1);
                    barrels[i] = endBarrel;
                    break;
                }

                int value = averageValue - baseRamdomItems[i].y;
                int fillValue = fillRamdomItems[fillIndex].y - value;

                int baseWeight = (int)(baseRamdomItems[i].y * 10000 / (float)averageValue);
                int3 weightBarrel = new int3(baseRamdomItems[i].x, baseWeight, fillRamdomItems[fillIndex].x); 
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
                int3 weightBarrel = new int3(baseRamdomItems[i].x, 10000, 0); 
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
     
    public string itemText;
    public bool isGroup;
    public int itemValue;
    public int randomValue;
    public int maxCount;
    public int minCount;

}
#endif


[System.Serializable]
public class GameRandomData
{
    public string text;
    public int id;
    public bool weightRandom;

    public List<RandomItem> randomItems;

    public List<int3> barrels;

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
        int3[] barrels = new int3[randomItems.Count];

        if (fillRamdomItems.Count > 0)
        {
            for (int i = 0; i < barrels.Length; i++)
            {
                if (fillRamdomItems.Count > 0 && fillIndex >= fillRamdomItems.Count)
                {
                    int3 endBarrel = new int3(baseRamdomItems[i].x, 10000, -1); 
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

                int3 weightBarrel = new int3(baseRamdomItems[i].x, baseWeight, fillRamdomItems.Count > fillIndex ? fillRamdomItems[fillIndex].x : baseRamdomItems[i].x);
              
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
                int3 weightBarrel = new int3(baseRamdomItems[i].x, 10000, 0); 
                barrels[i] = weightBarrel;
            }
        }

        this.barrels = barrels.ToList();

    }
}



[System.Serializable]

public class RandomItem
{ 
    public string text; 
    public bool isGroup;
    public int itemValue;
    public int randomValue;
    public int maxCount;
    public int minCount;
   // public Vector2Int countRange;
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