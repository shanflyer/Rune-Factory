using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

[BurstCompile]
public class GameRandom : Singleton<GameRandom>
{
    public static int RandomInt(int min, int max)
    {
        var random = Random;
        int result = random.NextInt(min, max);
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

    public static float RandomFloat(float min, float max)
    {
        var random = Random;
        float result = random.NextFloat(min, max);
        Random = random;
        //randomSeed += (uint)result;
        //instance.random = new Random(randomSeed);
        return result;
    }

    public override async void Init()
    {
        base.Init();
        randomSeed = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        random = new Random(randomSeed);
        await LoadRandomDataList();
    }

    public void RefreshRandomSeed(ref uint randomSeed)
    {
        if (randomSeed == 0)
        {
            randomSeed = (uint)(DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
        }
        random = new Random(randomSeed);
    }

    private Dictionary<int, GameRandomData> gameRandomDatas = new Dictionary<int, GameRandomData>();

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

        for (int i = 0; i < gameRandomDataList.gameRandomDatas.Count; i++)
        {
            var gameRandomData = gameRandomDataList.gameRandomDatas[i];
            gameRandomDatas[gameRandomData.id] = gameRandomData;
        }
    }

    public List<int2> GetRandomValue(int id, int innerGroupCount = 0, int randomResultCount = 1, bool temp = false, float countValue = -1)
    {
        if (gameRandomDatas.TryGetValue(id, out var gameRandomData))
        {
            return GetRandomValue(gameRandomData, innerGroupCount, randomResultCount, countValue);
        }
        return null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="gameRandomData">随机数据</param>
    /// <param name="innerGroupCount">嵌套层数</param>
    /// <param name="randomResultCount">试图获取的数量</param>
    /// <returns></returns>
    public List<int2> GetRandomValue(GameRandomData gameRandomData, int innerGroupCount = 0, int randomResultCount = 1, float countValue = -1)
    {
        if (innerGroupCount > 5)
        {
            return null;
        }
        List<int2> randomResults = new List<int2>();
        if (gameRandomData.weightRandom)
        {
            MyList<int3> weightBarrels = new MyList<int3>(gameRandomData.barrels);
            HashSet<int> haveGetItems = new HashSet<int>();
            RandomBarrelAction(randomResultCount);
            void RandomBarrelAction(int nowRandomResultCount)
            {
                if (weightBarrels.length == 0)
                {
                    weightBarrels.SetList(gameRandomData.barrels);
                }

                int index = RandomInt(0, weightBarrels.length);
                var barrel = weightBarrels[index];
                int randomValue = RandomInt(0, 10000);
                weightBarrels.RemoveAt(index);

                RandomItem randomItem;
                if (haveGetItems.Contains(barrel.x) && haveGetItems.Contains(barrel.z))
                {
                    RandomBarrelAction(nowRandomResultCount);
                    return;
                }
                if (randomValue < barrel.y)
                {
                    randomItem = gameRandomData.randomItems[barrel.x];
                    haveGetItems.Add(barrel.x);
                }
                else
                {
                    randomItem = gameRandomData.randomItems[barrel.z];
                    haveGetItems.Add(barrel.z);
                }

                if (!randomItem.isGroup)
                {
                    int count = RandomInt(randomItem.minCount, randomItem.maxCount);
                    int2 randomResult = new int2(randomItem.itemValue, count);
                    randomResults.Add(randomResult);

                    nowRandomResultCount -= count;
                    if (nowRandomResultCount > 0)
                    {
                        RandomBarrelAction(nowRandomResultCount);
                    }
                }
                else
                {
                    if (gameRandomDatas.TryGetValue(randomItem.itemValue, out var randomData))
                    {
                        var result = GetRandomValue(randomData, innerGroupCount++, nowRandomResultCount, countValue);
                        if (result != null)
                        {
                            randomResults.AddRange(result);
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < gameRandomData.randomItems.Count; i++)
            {
                var randomItem = gameRandomData.randomItems[i];
                int randomValue = RandomInt(0, 10000);
                if (randomValue < randomItem.randomValue)
                {
                    if (!randomItem.isGroup)
                    {
                        int count = RandomInt(randomItem.minCount, randomItem.maxCount);
                        int2 randomResult = new int2(randomItem.itemValue, count);
                        randomResults.Add(randomResult);
                    }
                    else
                    {
                        if (gameRandomDatas.TryGetValue(randomItem.itemValue, out var randomData))
                        {
                            var result = GetRandomValue(randomData, innerGroupCount++, randomResultCount, countValue);
                            if (result != null)
                            {
                                randomResults.AddRange(result);
                            }
                        }
                    }
                }
            }
        }

        return randomResults;
    }
}