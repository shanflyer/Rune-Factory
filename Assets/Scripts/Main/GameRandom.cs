using System;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Random = Unity.Mathematics.Random;
using UnityEngine.UI;
using UnityEditor.Rendering;

[System.Serializable]
public struct RandomResult
{
    public string result;
    public int count; 

    public override string ToString()
    {
        return $"{result}:{count}";
    }
}
public class GameRandom:Singleton<GameRandom>
{
    public static int RandomInt(int min,int max)
    {
        var random = Random;
        int result= random.NextInt(min, max);
        Random = random;
        return result;
    }
    public static int2 RandomInt2(int2 min, int2 max)
    {
        var random = Random;
        int2 result = random.NextInt2(min, max);
        Random = random;
        return result;
    }
    public static float RandomFloat(float min,float max)
    {
        var random = Random;
        float result= random.NextFloat(min, max);
        Random = random;
        //randomSeed += (uint)result;
        //instance.random = new Random(randomSeed);
        return result;
    }


    [BurstCompile]
    public struct GameRandomJobData:IJob
    {
         [ReadOnly]
        public Random random;
        [ReadOnly]
        public bool weightRandom;
        [ReadOnly]
        public int randomResultCount;
        [ReadOnly]
        public NativeArray<RamdomItemJobData> randomItems;
      
        public NativeList<WeightBarrel> barrels;
        [WriteOnly]
        public NativeList<RandomJobResult> randomResults;

        public GameRandomJobData(GameRandomData gameRandomData,int randomResultCount, Random random)
        {
            this.random = random;
            weightRandom = gameRandomData.weightRandom;
            this.randomResultCount = randomResultCount;
            randomItems = new NativeArray<RamdomItemJobData>(gameRandomData.randomItems.Count, Allocator.TempJob);
            barrels = new NativeList<WeightBarrel>(gameRandomData.barrels.Count, Allocator.TempJob);
            randomResults = new NativeList<RandomJobResult>(Allocator.TempJob);

            for(int i = 0; i < gameRandomData.randomItems.Count; i++)
            {
                randomItems[i] = new RamdomItemJobData(gameRandomData.randomItems[i]);
            }
            for (int i = 0; i < gameRandomData.barrels.Count; i++)
            {
                barrels.Add(gameRandomData.barrels[i]);
            }

        }

        public void Execute()
        {  
            int nowRandomJobResult = 0;
            
            if (weightRandom)
            {
                while (nowRandomJobResult < randomResultCount && barrels.Length > 0)
                { 
                    
                    int randomIndex = random.NextInt(0, barrels.Length);
                    int randomValue = random.NextInt(0, 10000);
                    RamdomItemJobData randomItem;
                    WeightBarrel weightBarrel = barrels[randomIndex];

                    if (barrels[randomIndex].baseWeight > randomValue)
                    {
                        randomItem = randomItems[weightBarrel.itemIndex];
                    }
                    else
                    {
                        randomItem = randomItems[weightBarrel.fillItemIndex];
                    }
                    barrels.RemoveAt(randomIndex);

                    int count = random.NextInt(randomItem.minCount, randomItem.maxCount + 1);
                    RandomJobResult randomResult = new RandomJobResult
                    {
                        result = randomItem.itemId,
                        group= randomItem.isGroup,
                        count = count
                    };
                    randomResults.Add(randomResult);
                    nowRandomJobResult++;
                }


            }
            else
            {
                for (int i = 0; i < randomItems.Length; i++)
                {
                    RamdomItemJobData randomItem = randomItems[i];
                    int randomValue = random.NextInt(0, 10000); 
                    if (randomValue < randomItem.randomValue)
                    {
                        Random random2 = new Random();
                        int count = random2.NextInt(randomItem.minCount, randomItem.maxCount + 1);

                        RandomJobResult randomResult = new RandomJobResult
                        {
                            result = randomItem.itemId,
                            group = randomItem.isGroup,
                            count = count
                        };

                        randomResults.Add(randomResult);
                    }
                }

            }

        }
    }
    [BurstCompile]
    public struct RamdomItemJobData
    { 
        public int itemId;
        public bool isGroup; 
        public int randomValue;
        public int maxCount;
        public int minCount;

        public RamdomItemJobData(RandomItem randomItem)
        {
            itemId = randomItem.itemId;
            isGroup = randomItem.isGroup;
            randomValue = randomItem.randomValue;
            maxCount = randomItem.maxCount;
            minCount = randomItem.minCount;
        }
    }
    [BurstCompile]
    public struct RandomJobResult
    {
        public int result;
        public int count;
        public bool group; 
    }
    public override void Init()
    {
        base.Init();
        randomSeed = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        random = new Random(randomSeed);
        LoadRandomDataList();
    } 

    public void RefreshRandomSeed(ref uint randomSeed)
    {
        if (randomSeed == 0)
        {
            randomSeed = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        }
        random = new Random(randomSeed);
    }

    Dictionary<int, GameRandomData> gameRandomDatas = new Dictionary<int, GameRandomData>();
    Dictionary<int, string> randomItemValues = new Dictionary<int, string>();

    private static Random Random
    {
        get
        {
            if (random.state == 0)
            {
                randomSeed = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
                random = new Random(randomSeed);
            }
            return random;
        }
        set
        {
            random = value;
        }
    }
    private static Random random;
    private static uint randomSeed;
    
  
    public async Task LoadRandomDataList()
    {
        GameRandomDataList gameRandomDataList = await ExtensionsResources.LoadResourceAsync<GameRandomDataList>(
          $"{DataPath.GetDataPath(typeof(GameRandomDataList))}");
        gameRandomDatas.Clear();
        randomItemValues.Clear();

        for (int i = 0; i < gameRandomDataList.gameRandomDatas.Count; i++)
        {
            var gameRandomData = gameRandomDataList.gameRandomDatas[i];
            gameRandomDatas[gameRandomData.id] = gameRandomData;
            for(int j = 0; j < gameRandomData.randomItems.Count; j++)
            {
                var randomItem = gameRandomData.randomItems[j];
                randomItemValues[randomItem.itemId] = randomItem.itemValue;
            }
        }

    }

    public List<RandomResult> GetRandomValue(int id, int innerGroupCount = 0, int randomResultCount = 1)
    {
        List<RandomResult> randomResults = new List<RandomResult>();
        var jobResults = GetRandomJobValue(id, innerGroupCount, randomResultCount);
        for(int i = 0; i < jobResults.Length; i++)
        {
            var jobResult = jobResults[i];
            if(randomItemValues.TryGetValue(jobResult.result,out string itemValue))
            {
                randomResults.Add(new RandomResult
                {
                    result = itemValue,
                    count = jobResult.count
                });
            }
        }
        jobResults.Dispose();
        return randomResults;
    }

    [BurstCompile]
    NativeList<RandomJobResult> GetRandomJobValue(int id, int innerGroupCount = 0, int randomResultCount = 1)
    {
        NativeList<RandomJobResult> randomResults = new NativeList<RandomJobResult>(Allocator.Temp);
        if (innerGroupCount > GameCommon.randomInnerGroupMax)
        {
            Debug.LogError("随机嵌套超过5层！");
            return randomResults;
        }
        if (gameRandomDatas.TryGetValue(id, out GameRandomData gameRandomData))
        {
            GameRandomJobData gameRandomJobData = new GameRandomJobData(gameRandomData,randomResultCount,random);


            gameRandomJobData.Schedule().Complete();

            for(int i=0;i< gameRandomJobData.randomResults.Length; i++)
            {
                var randomResult = gameRandomJobData.randomResults[i];
                if (randomResult.group) 
                {
                    if(randomItemValues.TryGetValue(randomResult.result,out string randomItemValue))
                    {
                        int groupId = int.Parse(randomItemValue);
                        var _randomResults = GetRandomJobValue(groupId, innerGroupCount + 1);

                        randomResults.AddRangeNoResize(_randomResults);
                    }                   
                }
                else
                {
                    randomResults.Add(randomResult);
                }
            } 
           
        }

        return randomResults;
    } 
}
