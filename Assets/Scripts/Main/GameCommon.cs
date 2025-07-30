using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public delegate void Int3Action(int3 value, int action = 0);

public delegate Vector2 GetMoveVector();

public delegate void SetMoveTarge(int2 targetCoordinate, Vector2 targetPos);

[System.Serializable]
public enum AttributeType
{
    无 = 0,
    水 = 1,
    火 = 2,
    冰 = 3,
    风 = 4,
    土 = 5
}

[System.Serializable]
public enum Gender
{
    animal = 0,
    male = 1,
    female = 2
}

[System.Serializable]
public enum ValueType
{
    AT = 1,
    DF = 2,
    MaxHp = 3,
    Hp = 4,
    Exp = 5,
    Power = 6,
    MaxPower = 7,
    Crit = 8,
    Dodge = 9,
}

[System.Serializable]
public enum CompareType
{
    等于, 不等于, 大于, 不大于, 小于, 不小于
}

[System.Serializable]
public enum CharacterPropertyType
{
    自定义值 = -1, 体力 = 0, 生命 = 1, 法力 = 2, 攻击 = 3, 防御 = 4, 幸运 = 5, 饱食 = 6,
    最大体力 = 7, 最大生命 = 8, 最大法力 = 9, 敏捷 = 10
}

public enum Direction
{
    Default = -1, UP = 0, LEFT = 1, DOWN = 2, RIGHT = 3,
}

public enum RuntimeObjType
{
    MAPGROUND, MAPITEM, CHARACTER, STOREITEM, EMOTE, FISHTOOL
}

public enum FightRuntimeObjType
{
    PLAYER, FIGHTMAP, FIGHTITEM, MONSTRT, PLAYABLEDIRECTOR, OTHER
}

public static class CharacterAnimatorParameter
{
    public static int Fish = Animator.StringToHash("Fish");
    public static int Set = Animator.StringToHash("Set");
    public static int Speed = Animator.StringToHash("Speed");
    public static int Dir_X = Animator.StringToHash("Dir_X");
    public static int Dir_Y = Animator.StringToHash("Dir_Y");
}

public enum EntityType
{
    All = 1, 地图道具 = 2, 角色 = 15, 玩家 = 3
}

public static class AttackType
{
    /// <summary>
    /// 默认
    /// </summary>
    public static int defaultAttack = 0;

    /// <summary>
    /// 刀剑攻击
    /// </summary>
    public static int swordAttack = 1;

    /// <summary>
    /// 长矛攻击
    /// </summary>
    public static int spearAttack = 2;

    /// <summary>
    /// 斧子攻击
    /// </summary>
    public static int axeAttack = 3;

    /// <summary>
    /// 咬
    /// </summary>
    public static int biteAttack = 4;

    /// <summary>
    /// 爪
    /// </summary>
    public static int pawAttack = 5;

    /// <summary>
    /// 鞭
    /// </summary>
    public static int whipAttack = 6;
}
public static class SpanStringReplacer
{
    /// <summary>
    /// 使用 Span 高效替换多个子字符串
    /// </summary>
    public static string ReplaceMultipleStrings(string input,
        Dictionary<string, string> replacements,
        StringComparison comparison = StringComparison.Ordinal)
    {
        if (string.IsNullOrEmpty(input)) return input;
        if (replacements == null || replacements.Count == 0) return input;

        // 预处理：按长度和优先级排序
        var sortedReplacements = replacements
            .OrderByDescending(kv => kv.Key.Length)
            .ThenByDescending(kv => kv.Key)
            .ToArray();

        // 使用 ValueStringBuilder 减少内存分配
        var sb = new ValueStringBuilder(stackalloc char[256]);
        ReadOnlySpan<char> inputSpan = input.AsSpan();
        int index = 0;

        while (index < inputSpan.Length)
        {
            bool replaced = false;

            // 尝试匹配所有可能的替换项
            foreach (var (oldValue, newValue) in sortedReplacements)
            {
                if (oldValue.Length == 0) continue;

                // 检查当前位置是否匹配
                if (IsMatchAt(inputSpan, index, oldValue.AsSpan(), comparison))
                {
                    sb.Append(newValue);
                    index += oldValue.Length;
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                sb.Append(inputSpan[index]);
                index++;
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 检查指定位置是否匹配目标子串
    /// </summary>
    private static bool IsMatchAt(
        ReadOnlySpan<char> source,
        int startIndex,
        ReadOnlySpan<char> target,
        StringComparison comparison)
    {
        if (startIndex + target.Length > source.Length) return false;

        return comparison switch
        {
            StringComparison.Ordinal => source.Slice(startIndex, target.Length).SequenceEqual(target),
            StringComparison.OrdinalIgnoreCase => EqualsOrdinalIgnoreCase(
                source.Slice(startIndex, target.Length), target),
            _ => source.Slice(startIndex, target.Length).ToString().Equals(
                target.ToString(), comparison)
        };
    }

    /// <summary>
    /// 高性能的 OrdinalIgnoreCase 比较
    /// </summary>
    private static bool EqualsOrdinalIgnoreCase(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
        {
            if (char.ToUpperInvariant(a[i]) != char.ToUpperInvariant(b[i]))
                return false;
        }
        return true;
    }
}

// ValueStringBuilder（简化版，实际使用可引用 System.Text.Private.CoreLib）
internal ref struct ValueStringBuilder
{
    private Span<char> _buffer;
    private int _length;

    public ValueStringBuilder(Span<char> initialBuffer)
    {
        _buffer = initialBuffer;
        _length = 0;
    }

    public void Append(ReadOnlySpan<char> value)
    {
        if (value.Length == 0) return;
        EnsureCapacity(_length + value.Length);
        value.CopyTo(_buffer.Slice(_length));
        _length += value.Length;
    }

    public void Append(char c)
    {
        EnsureCapacity(_length + 1);
        _buffer[_length++] = c;
    }

    private void EnsureCapacity(int requiredCapacity)
    {
        if (requiredCapacity <= _buffer.Length) return;
        Span<char> newBuffer = new char[Math.Max(requiredCapacity, _buffer.Length * 2)];
        _buffer.CopyTo(newBuffer);
        _buffer = newBuffer;
    }

    public override string ToString() => _buffer.Slice(0, _length).ToString();
}
public static class GameCommon
{
    public static List<MyString> GetMyStrings(this List<string> strs)
    {
        List<MyString> myStrings = new List<MyString>();
        for (int i = 0; i < strs.Count; i++)
        {
            myStrings.Add(new MyString { value = strs[i] });
        }
        return myStrings;
    }
    public static Dictionary<Direction, Vector2> fishToolOffsets = new Dictionary<Direction, Vector2>
    {
        {Direction.LEFT,new Vector2(-0.556f,0.034f)},
        {Direction.RIGHT,new Vector2(0.556f,0.034f)},
        {Direction.UP,new Vector2(0,0.6531f)},
        {Direction.DOWN,new Vector2(0,-0.39f)},
    };
  
    public const int AddATBuff = 11;
    public const int AddDFBuff = 12;
    public const int AddSpeedBuff = 13;
    public const int AddLuckyBuff = 14;

    public static float2 fishWaitCD = new float2(4.0f, 12.0f);
    public static float2 autoCustomerCD = new float2(4.0f, 20.0f);
    public const int MyPlayerStore = 547;
    public const int timeStoreCurveData = 12;
    public const int weatherStoreCurveData = 13;

    public const int BlueObjLayer = 13;
    public const int GreenObjLayer = 14;
    public const int RedObjLayer = 15;

    public const int ManufatureWorkingEmote = 72;
    public const int ManufatureWorkendEnote = 14;      

    public const int PixelCameraDefaultValue = 400;
    public const int setTeamerFunctionId = 4;
    public const int defaultProduct = 1;
    public const int explorCostMinute = 120;
    public const int exploreCostPower = 4;
    public const float fightCharacterMoveTime = 0.25f;
    public static int2 fightWalkTime = new int2(4000, 7000);
    public const int giftEventId = 412;
    public const int zeroGameYear = 1300;
    public const Season zeroSeasom = Season.春;
    public const int zeroDay = 1;
    public const float sleepCostTime = 6.0f;
    public const int shortcutItemCount = 6;

    public const float freedomMoveValue = 1.2f;

    public const int animalDefaultFoodItem = 70;
    public const int selectEquipBoxAction = 90;
    public const int defaultGiftTalk = 8888;
    public const int defaultPerRPCost = 5;
    public const int storeCoinTime = 1000;
    public const float HurtUtlility = 0.6f;
    public const float hpUtlility = 0.68f;

    public const float DefaultPerRoundCd = 1.0f;

    public static int3 friendAddCount = new int3(3, 3, 2);

    public static List<int> zeroNPC = new List<int>
    {
        1001,2001
    }; 
    //组队
    public const int TeamFull = 5002;//人太多
    public const int TeamHurt = 5003;//受伤
    public const int TeamLeave = 5004;//离开

    public const string backHomeBehaviorPath = "Behavior/NPC/New/家里闲逛";
    //钓鱼
    public const int GetFish = 41;//收竿
    public const int StartFish = 40;//钓鱼
    public const int GetFishEmote = 0;
    public const int NotGetFishEmote =24;

    //土地
    public const int SmoothField = 10;//锄地
    public const int Seeding = 20;//播种
    public const int Watering = 21;//浇水
    public const int Harvesting = 22;//收获
    public const int Reaping = 26;//收割
    public const int Eradicate = 27;//铲除
    public const int dryPlantEmote = 26;
    public const int fritEmote = 63;
    public const int plantDeath = 40;

    public const int animalNeedFood = 32;


    public static int defaultOperateId = 0;
    public static int defaultPlayerTalkTime = 2000;
    public static int grassItem = 100;

    public const int fishSuccessEmote = 61;
    public const int fishFailedmote = 24;

    public static float2 dropWaitTime = new float2(0.4f, 0.8f);
    public static float4 dropArea = new float4(-0.6f, -0.3f, 0.6f, 0.3f);
    public const float dropItemFlyerSpeed = 3f;
    public const int SeasonDays = 30;
    public const float fightMapMovingSpeed = 1f;

    public const int DefaultOutItemId = -1;
    public const float PromptTime = 2.0f;
    public const float cellWidth = 0.08f, cellHigh = 0.08f;
    public const float cellSize = 0.04f;
    public const float oneDividCellWidth = 12.5f, oneDividCellHigh = 12.5f;
    public const float slantValue = 0.707f;

    public const int randomInnerGroupMax = 5;

    public const int worldMapSizeX = 80;
    public const int worldMapSizeY = 45;
    public const float worldMapTileSize = 0.08f;

    public const float hightMin = -0.5f;
    public const float hightMax = 1;

    public const float waterPerlinMin = 4;
    public const float waterPerlinMax = 6;

    public const float hightPerlinMin = 8;
    public const float hightPerlinMax = 10;

    public const int worldGridTypeSeedMin = 200;
    public const int worldGridTypeSeedMax = 2000;

    public const int hightSeedMin = 100;
    public const int hightSeedMax = 400;

    public const int waterSeedMin = 1000;
    public const int waterSeedMax = 10000;

    public const float waterMin = 0.01f;
    public const float waterMax = 0.02f;

    public const float waterRandomMin = 0;
    public const float waterRandomMax = 1;

    /// <summary>
    /// 生成水面的噪声时对结果的重映射
    /// </summary>
    public const float waterLerpValueMin_Min = 0.5f;

    public const float waterLerpValueMin_Max = 1f;
    public const float waterLerpValueMax_Min = 1f;
    public const float waterLerpValueMax_Max = 2.0f;

    public const float mapChangeLerpTime = 0.4f;

    public const float ScreenHalfSizeX = 960;
    public const float ScreenHalfSizeY = 540;

    public const int fishingGameTime = 15000;

    public const string characterTriggerRenferenceName = "CharacterId";
    public const string triggerRenferenceName = "Reference";

    public const string PlayerBoxId = "PlayerBoxId";

    public const float weatherLerpTime = 5.0f;

    public static Vector2 GetScreenResolution()
    {
      //  return new Vector2(Screen.width, Screen.height);
        Vector2 gameViewSize;
        //使用宏编译主要是为了打包的时候不会报错
#if UNITY_EDITOR
        gameViewSize = GameViewSize();
#else
        gameViewSize = new Vector2(Screen.currentResolution.width,
            Screen.currentResolution.height) ;
#endif

        return gameViewSize;
    }

#if UNITY_EDITOR

    private static Vector2 GameViewSize()
    {
        var mouseOverWindow = UnityEditor.EditorWindow.mouseOverWindow;
        System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
        System.Type type = assembly.GetType("UnityEditor.PlayModeView");

        Vector2 size = (Vector2)type.GetMethod(
            "GetMainPlayModeViewTargetSize",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Static
        ).Invoke(mouseOverWindow, null);

        return size;
    }

#endif

    public static List<int2> GridToCells(List<int> grid)
    {
        int gridCount = grid.Count / 4;

        List<int2> cells = new List<int2>();
        for (int i = 0; i < gridCount; i++)
        {
            int minX = grid[i * 4];
            int minY = grid[i * 4 + 1];

            int maxX = grid[i * 4 + 2];
            int maxY = grid[i * 4 + 3];
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    cells.Add(new int2(x, y));
                }
            }
        }
        return cells;
    }
    public static List<int> CellToGrid(List<int2> cells)
    { 
        List<int> result = new List<int>();

        HashSet<int2> allCellPoints = new HashSet<int2>();

        for (int i = 0; i < cells.Count; i++)
        {
            var cell = cells[i];
            allCellPoints.Add(cell);
        }


        while (allCellPoints.Count > 0)
        {
            int2 startPoint = new int2(int.MinValue, int.MinValue);
            using (var e = allCellPoints.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    startPoint = e.Current;
                }
            }
            if (startPoint.x != int.MinValue)
            {
                int left = startPoint.x;
                int right = startPoint.x;
                int bottom = startPoint.y;
                int top = startPoint.y;

                bool match = true;
                while (match)
                {
                    left--;
                    int2 leftPoint = new int2(left, startPoint.y);
                    if (!allCellPoints.Contains(leftPoint))
                    {
                        left++;
                        match = false;
                        break;
                    }
                }

                match = true;
                while (match)
                {
                    right++;
                    int2 rightPoint = new int2(right, startPoint.y);
                    if (!allCellPoints.Contains(rightPoint))
                    {
                        right--;
                        match = false;
                        break;
                    }
                }

                match = true;
                while (match)
                {
                    bottom--;
                    for (int x = left; x <= right; x++)
                    {
                        int2 point = new int2(x, bottom);
                        if (!allCellPoints.Contains(point))
                        {
                            match = false;
                            bottom++;
                            break;
                        }
                    }
                }

                match = true;
                while (match)
                {
                    top++;
                    for (int x = left; x <= right; x++)
                    {
                        int2 point = new int2(x, top);
                        if (!allCellPoints.Contains(point))
                        {
                            match = false;
                            top--;
                            break;
                        }
                    }
                }

                for (int x = left; x <= right; x++)
                {
                    for (int y = bottom; y <= top; y++)
                    {
                        int2 point = new int2(x, y);
                        allCellPoints.Remove(point);
                    }
                }
                result.Add(left);
                result.Add(bottom);
                result.Add(right);
                result.Add(top);
            }
        }
        allCellPoints.Clear();
        cells.Clear();
        return result;
    }


    public static void RemoveValue<TKey, TValue>(
    ref NativeParallelMultiHashMap<TKey, TValue> map,
    TKey key,
    TValue targetToRemove)
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged, IEquatable<TValue>
    {
        if (!map.TryGetFirstValue(key, out var value, out var it)) return;

        var newValues = new NativeList<TValue>(Allocator.Temp);

        do
        {
            if (!value.Equals(targetToRemove))
                newValues.Add(value);
        }
        while (map.TryGetNextValue(out value, ref it));

        // 清空旧值
        map.Remove(key);

        // 重新添加非目标项
        for (int i = 0; i < newValues.Length; i++)
            map.Add(key, newValues[i]);

        newValues.Dispose();
    }

    public static int2 GetDirectionInt2(Direction direction)
    {
        int2 value = int2.zero;
        switch (direction)
        {
            case Direction.UP:
                value = new int2(0, 1);
                break;

            case Direction.LEFT:
                value = new int2(-1, 0);
                break;

            case Direction.DOWN:
                value = new int2(0, -1);
                break;

            case Direction.RIGHT:
                value = new int2(1, 0);
                break;
        }
        return value;
    }


    
    /// <summary>
    /// 转换方向为值
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static int GetDirectionValue(Direction direction)
    {
        var value = 1;
        for (int i = 0; i < (int)direction; i++)
        {
            value *= 2;
        }
        return value;
    }

    /// <summary>
    /// 检查方向值
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool CheckDirectionValue(Direction direction, int target)
    {
        var value = Convert.ToString(target, 2).ToCharArray();
        int index = (int)direction;
        try
        {
            return value[value.Length - 1 - index] == '1';
        }
        catch
        {
            return false;
        }
    }

    public static int2 StringToInt2(string str)
    {
        int2 result = new int2();
        try
        {
            if (str.Length > 4)
            {
                str = str.Substring(4, str.Length - 4);
                var strs = str.Split(',');
                result.x = int.Parse(strs[0]);
                result.y = int.Parse(strs[1]);
            }
        }
        catch { }

        return result;
    }

    public static Vector3 StringToVector3(string str)
    {
        Vector3 vector3 = new Vector3();
        try
        {
            if (str.Length > 2)
            {
                str = str.Substring(1, str.Length - 1);
                var strs = str.Split(',');
                vector3.x = float.Parse(strs[0]);
                vector3.y = float.Parse(strs[1]);
                vector3.z = float.Parse(strs[2]);
            }
        }
        catch { }

        return vector3;
    }
    public static List<int> StringToListInt(string str)
    {
        List<int> result = new List<int>();
        try
        {
            var strs = str.Split(',');
            for(int i = 0; i < strs.Length; i++)
            {
                result.Add(int.Parse(strs[i]));
            }
        }
        catch { }
        return result;
    }
    public static int3 StringToInt3(string str)
    {
        int3 int3 = new int3();
        try
        {
            if (str[0] =='i')
            {
                str = str.Substring(5, str.Length - 6);
                var strs = str.Split(',');
                int3.x = int.Parse(strs[0]);
                int3.y = int.Parse(strs[1]);
                int3.z = int.Parse(strs[2]);
            }
            else
            {
                var strs = str.Split(',');
                int3.x = int.Parse(strs[0]);
                int3.y = int.Parse(strs[1]);
                int3.z = int.Parse(strs[2]);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

        return int3;
    }

    public static Vector2 SetImageSize(Sprite sprite, Vector2 size)
    {
        if (sprite == null)
        {
            return size;
        }
        Vector2 spriteSize = sprite.rect.size;
        if (spriteSize.x > spriteSize.y)
        {
            float value = size.x / spriteSize.x;
            float y = spriteSize.y * value;
            return new Vector2(size.x, y);
        }
        else
        {
            float value = size.y / spriteSize.y;
            float x = spriteSize.x * value;
            return new Vector2(x, size.y);
        }
    }

    public static float GetCellTrueDistance(int2 coordinate0, int2 coordinate1)
    {
        int2 result = coordinate0 - coordinate1;
        return math.length(new float2(result.x * cellWidth, result.y * cellHigh));
    }

    public static int GetCellDistance(int2 coordinate0, int2 coordinate1)
    {
        int2 result = coordinate0 - coordinate1;
        return math.abs(result.x) + math.abs(result.y);
    }

    public static string AddString(string s0, string s1)
    {
        s1 = s1.Replace("_", "/");
        var span = s1.AsSpan();
      
        var builder = new StringBuilder(s0);
        builder.Append(span);
        return builder.ToString();
    }
    public static string BlendString(string s0, params string[] args)
    {  
        var builder = new StringBuilder(s0);
        for (int i = 0; i < args.Length; i++)
        {
            var span = args[i].ToString().AsSpan();
            builder.Append(span);
        } 
        return builder.ToString();
    }
    public static void SetEnable(GameObject gameObject, bool enable, bool compontEnable)
    {
        gameObject.transform.localScale = enable ? Vector3.one : Vector3.zero;
    }

    public static int NextRandom(int numSeeds, int length)
    {
        // Create a byte array to hold the random value.
        byte[] randomNumber = new byte[length];
        // Create a new instance of the RNGCryptoServiceProvider.
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        // Fill the array with a random value.
        rng.GetBytes(randomNumber);
        // Convert the byte to an uint value to make the modulus operation easier.
        uint randomResult = 0x0;
        for (int i = 0; i < length; i++)
        {
            randomResult |= ((uint)randomNumber[i] << ((length - 1 - i) * 8));
        }
        return (int)(randomResult % numSeeds) + 1;
    }

    public static int CreateRandSeed()
    {
        var iSeed = 0;
        var guid = Guid.NewGuid();
        iSeed = guid.GetHashCode();
        return iSeed;
    }

    public static float2 GetDirectValue(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                return new float2(0, 1);

            case Direction.LEFT:
                return new float2(-1, 0);

            case Direction.RIGHT:
                return new float2(1, 0);

            case Direction.DOWN:
                return new float2(0, -1);
        }
        return float2.zero;
    }

    public static Direction GetDirect(int2 start, int2 target)
    {
        int2 offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        else
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }
    }

    public static Direction GetDirect(Vector2Int start, Vector2Int target)
    {
        Vector2Int offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        else
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }
    }
    public static float VectorAngle(Vector2 from, Vector2 to)
    {
        float angle;
        Vector3 cross = Vector3.Cross(from, to);
        angle = Vector2.Angle(from, to);
        return cross.z > 0 ? angle : -angle;
    }

    private static float tansMin = math.tan(math.radians(22.5f));
    private static float tansMax = math.tan(math.radians(67.5f));

    private static float2 Left = new float2(-1, 0);
    private static float2 Right = new float2(1, 0);
    private static float2 Up = new float2(0, 1);
    private static float2 Down = new float2(0, -1);

    private static float2 LeftUp = new float2(-1, 1);
    private static float2 RightUp = new float2(1, 1);
    private static float2 LeftDown = new float2(-1, -1);
    private static float2 RightDown = new float2(1, -1);

    public static float2 InitMoveDirect(float2 movedirect)
    {
        if (movedirect.x == 0 && movedirect.y == 0)
        {
            return movedirect;
        }
        float value = math.abs(movedirect.y / movedirect.x);
        if (value < tansMin)
        {
            if (movedirect.x < 0)
            {
                return Left;
            }
            else
            {
                return Right;
            }
        }
        else if (value > tansMax)
        {
            if (movedirect.y < 0)
            {
                return Down;
            }
            else
            {
                return Up;
            }
        }
        else if (movedirect.x < 0)
        {
            if (movedirect.y < 0)
            {
                return LeftDown;
            }
            else
            {
                return LeftUp;
            }
        }
        else
        {
            if (movedirect.y < 0)
            {
                return RightDown;
            }
            else
            {
                return RightUp;
            }
        }
    }

    public static Direction GetCharacterDirect(float2 offset, Direction oldDirection = Direction.Default)
    {
        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.LEFT;
            }
            else
            {
                return Direction.RIGHT;
            }
        }
        if (Mathf.Abs(offset.x) < Mathf.Abs(offset.y))
        {
            if (offset.y < 0)
            {
                return Direction.DOWN;
            }
            else
            {
                return Direction.UP;
            }
        }
        if (offset.x != 0 && offset.y != 0)
        {
            if (offset.x < 0)
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
                else
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
            }
            else
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
                else
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
            }
        }
        return oldDirection;
    }

    public static Direction GetCharacterDirect(int2 start, int2 target, Direction oldDirection = Direction.Default)
    {
        int2 offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        if (Mathf.Abs(offset.x) < Mathf.Abs(offset.y))
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }
        if (offset.x != 0 && offset.y != 0)
        {
            if (offset.x < 0)
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
            else
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
        }
        return oldDirection;
    }

    public static Direction GetCharacterDirect(Vector2Int start, Vector2Int target, Direction oldDirection = Direction.Default)
    {
        Vector2Int offset = start - target;

        if (Mathf.Abs(offset.x) > Mathf.Abs(offset.y))
        {
            if (offset.x < 0)
            {
                return Direction.RIGHT;
            }
            else
            {
                return Direction.LEFT;
            }
        }
        if (Mathf.Abs(offset.x) < Mathf.Abs(offset.y))
        {
            if (offset.y < 0)
            {
                return Direction.UP;
            }
            else
            {
                return Direction.DOWN;
            }
        }
        if (offset != Vector2Int.zero)
        {
            if (offset.x < 0)
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.RIGHT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
            else
            {
                if (offset.y < 0)
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.UP)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.UP;
                    }
                }
                else
                {
                    if (oldDirection == Direction.LEFT || oldDirection == Direction.DOWN)
                    {
                        return oldDirection;
                    }
                    else
                    {
                        return Direction.DOWN;
                    }
                }
            }
        }
        return oldDirection;
    }

    public static float2 WorldCoordinateToPos(float2 coordinate)
    {
        float2 pos = new float2(coordinate.x * worldMapTileSize + worldMapTileSize * 0.5f, coordinate.y * worldMapTileSize + worldMapTileSize * 0.5f);
        return pos;
    }

    public static Vector2 GetMapPos(int x, int y)
    {
        Vector2 pos = new Vector2(cellWidth * x + cellWidth * 0.5f, cellHigh * y + cellHigh * 0.5f);
        return pos;
    }

    public static Vector2 GetMapPos(int2 coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x + cellWidth * 0.5f, cellHigh * coordinate.y + cellHigh * 0.5f);
        return pos;
    }

    public static Vector2 GetMapPos(Vector2Int coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x + cellWidth * 0.5f, cellHigh * coordinate.y + cellHigh * 0.5f);
        return pos;
    }

    public static Vector2 GetMapPos(Vector2 coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x + cellWidth * 0.5f, cellHigh * coordinate.y + cellHigh * 0.5f);
        return pos;
    }

    public static Vector2 GetZeroMapPos(Vector2Int coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x, cellHigh * coordinate.y);
        return pos;
    }

    public static Vector2 GetZeroMapPos(int2 coordinate)
    {
        Vector2 pos = new Vector2(cellWidth * coordinate.x, cellHigh * coordinate.y);
        return pos;
    }

    public static Vector2Int GetMapCoordinate(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x * oneDividCellWidth);
        int y = Mathf.FloorToInt(pos.y * oneDividCellHigh);

        return new Vector2Int(x, y);
    }

    public static int2 GetMapCoordinateInt(Vector2 pos)
    {
        int x = Mathf.FloorToInt(pos.x * oneDividCellWidth);
        int y = Mathf.FloorToInt(pos.y * oneDividCellHigh);

        return new int2(x, y);
    }

    public static int2 GetMapCoordinateInt(float2 pos)
    {
        int x = Mathf.FloorToInt(pos.x * oneDividCellWidth);
        int y = Mathf.FloorToInt(pos.y * oneDividCellHigh);

        return new int2(x, y);
    }

    public static bool CompareGameTime(int year, int season, int day, int hour, int minute, int targetYear, int targetSeason, int
        targetDay, int targetHour, int targetMinute)
    {
        if (targetYear > 0)
        {
            if (targetYear < year)
                return true;
            if (targetYear > year)
                return false;
        }
        if (targetSeason > 0)
        {
            if (targetSeason < season)
                return true;
            if (targetSeason > season)
                return false;
        }
        if (targetDay > 0)
        {
            if (targetDay < day)
                return true;
            if (targetDay > day)
                return false;
        }
        if (targetHour > 0)
        {
            if (targetHour < hour)
                return true;
            if (targetHour > hour)
                return false;
        }
        if (targetMinute > 0)
        {
            if (targetMinute <= minute)
                return true;
            if (targetMinute > minute)
                return false;
        }

        return true;
    }
}

public static class DefaultGameData
{
    public const int defaultDay = 1;
    public const int defaultHour = 8;
}

public static class EditorDataPath
{
    public const string itemIconPath = "Item/";

    public const string npcBehaviorPath = "Assets/Resources/Behavior/NPC/";
    public const string outDataPath = "Assets/Resources/Data/";
    public const string groundSourcePath = "Assets/Texture/Map/Ground/";
    public const string sourceChangeNameDataPath = "Assets/Editor/Data/SourceChangeName.json";

    public const string mapItemDataPath = "Assets/Resources/Data/MapItemData/";
    public const string mapRoomDataPath = "Assets/Resources/Data/MapRoomData/";
    public const string worldMapDataPath = "Assets/Resources/Data/WorldMapData/";

    public const string mapItemSourcePath = "Assets/Texture/Map/Item/";
    public const string mapItemPrefabPath = "Assets/Resources/Prefabs/MapItem/";
    public const string mapGroundPath = "Assets/Resources/Prefabs/Ground/";

    public const string colliderTile = "Assets/TileMap/Tiles/Event/0.asset";
    public const string triggerTile = "Assets/TileMap/Tiles/Event/e.asset";
    public const string playerTriggerTile = "Assets/TileMap/Tiles/Event/1.asset";

    public const string mapItemStructDataPath = "Assets/Editor/Data/MapItemStruct.json";
    public const string mapItemAnimationPath = "Assets/Animation/MapItem/";
    public const string gameEventDataPath = "Assets/Resources/Behavior/";
    public const string tempCharacterBehaviorPath = "Assets/Resources/Behavior/TempCharacter/";
    public const string npcCharacterBehaviorPath = "Assets/Resources/Behavior/NPC/";
}

public static class DataPath
{
    public static readonly Dictionary<Type, string> dataPathDic = new Dictionary<Type, string>
    {
        {typeof(GameGlobalData),"Data/GameGlobalData" },
        {typeof(LanguageSwitchDataList),"Data/LanguageSwitchData" },
        {typeof(FunctionData),"Data/FunctionDataList" },
        {typeof(GameEventData),"Data/GameEventData" },
        {typeof(GameRandomDataList),"Data/GameRandomData/GameRandomDataList" },
        {typeof(GrowModelData),"Data/GrowModelDataList" },
        {typeof(ItemData),"Data/ItemData" },
        {typeof(ItemAnimationData),"Data/ItemAnimationData" },
        {typeof(MapNpcDataList),"Data/MapNpcData/MapNpcDataList" },
        {typeof(MapRoomData),"Data/MapRoomData" },
        {typeof(MonsterData),"Data/MonsterData" },
        {typeof(ProfessionData),"Data/ProfessionData/ProfessionDataList" },
        {typeof(WorldMapData),"Data/WorldMapData" },
        {typeof(CharacterData),"Data/CharacterData" },
        {typeof(CharacterGroupData),"Data/CharacterGroupData" },
        {typeof(FightMapData),"Data/FightMapData/FightMapDataList" },
        {typeof(MonsterDeploy),"Data/MonsterDeploy/MonsterDeloyList" },
        {typeof(GameActionData),"Data/GameActionData" },
        {typeof(TalkData),"Data/TalkData" },
        {typeof(MyTimeLineData),"Data/TimeLineData" },
        {typeof(SkillData),"Data/SkillData" },
        {typeof(MapItemData),"Data/MapItemData" },
        {typeof(FormulaData),"Data/FormulaData" },
        {typeof(ManufactureData),"Data/ManufactureData" },
        {typeof(PackageSetData),"Data/PackageSetData" },
        {typeof(MoneyCreatData), "Data/MoneyCreatData" },
        {typeof(ShopGroup),"Data/ShopItemData/ShopDataList" },
        {typeof(TempCharacterCreateData),"Data/TempCharacterCreateData" },
        {typeof(OperateData),"Data/OperateData" },
        {typeof(StoreCounterData),"Data/StoreCounterData/StoreCounterDataList" },
        {typeof(FestivalData),"Data/FestivalData/FestivalDataList"},
        {typeof(PermissionData),"Data/PermissionData/PermissionDataList"},
        {typeof(TempCharacterData),"Data/TempCharacterData"},
        {typeof(NPCFunctionData), "Data/NPCFunctionData"},
        {typeof(NPCData),"Data/NPCData" },
        {typeof(FriendShipData),"Data/FriendShipData" },
        {typeof(PlantData),"Data/PlantData" },
        {typeof(FieldArea),"Data/FieldArea" },
        {typeof(PastureData),"Data/PastureData/PastureDataList" },
        {typeof(AnimalData),"Data/AnimalData" },
        {typeof(FishData),"Data/FishData" },
        {typeof(FishPondData),"Data/FishPondData" },
        {typeof(FilmData),"Data/FilmData" },
        {typeof(SeasonData),"Data/SeasonDataList" },
        {typeof(WeatherData),"Data/WeatherData" },
        {typeof(EnvironmentData),"Data/EnvironmentDataList" },
        {typeof(SleepSetData),"Data/SleepSetData/SleepSetDataList" },
        {typeof(SleepSetDataList),"Data/SleepSetData" },
        {typeof(GameTimeEventData),"Data/GameTimeEventData" },
        {typeof(EmoteData),"Data/EmoteData" },
        {typeof(SkyBackGroundData),"Data/SkyBackGroundData"},
        {typeof(HomeEquipmentData),"Data/HomeEquipmentData" },
        {typeof(ShopItemDisplayData),"Data/ShopItemDisplayData"},
        {typeof(ObjPackageAnimationData),"Data/ObjPackageAnimationData" },
        {typeof(BuffData),"Data/BuffData" },
        {typeof(NPCTaskScheduleData),"Data/NPCTaskScheduleData" },
        {typeof(ZeroInitDataList),"Data/ZeroInitDataList" },
        {typeof(TaskScheduleModelData),"Data/TaskScheduleModelDataList/TaskScheduleModelDataList" },
        {typeof(NPCBehaviorData),"Data/NPCBehaviorData" },
        {typeof(MulitiBehaviorData),"Data/MulitiBehaviorData" },
        {typeof(FootstepDataList),"Data/FootstepDataList" },
        {typeof(GameGuideData),"Data/GameGuideData" },
        {typeof(GameGuideFilmData),"Data/GameGuideFilmData" },
        {typeof(AppStoreProductData),"Data/AppStoreProductData" },
        {typeof(FunctionInfoData),"Data/FunctionInfoData" },
        {typeof(DefaultConfigData),"Data/DefaultConfigData" },
        {typeof(LanguageData),"Data/LanguageData" }
    };

    public static string GetDataPath(Type type)
    {
        if (dataPathDic.TryGetValue(type, out string path))
        {
            return path;
        }
        return null;
    }

    public const string fishToolPrefab = "Prefabs/Other/鱼漂";
    public const string StoreCoinPrefab = "Prefabs/Other/Coin";
    public const string StoreCounterPrefab = "Prefabs/Other/SellItem";
    public const string goldSpritePath = "Reference/Gold";
    public const string diamondSpritePath = "Reference/Diamond";

    public const string cameraPrefabPath = "Prefabs/Other/CameraObj";
    public const string InputDataPath = "InputData/MyInput";

    public const string pointerEffectPath = "Prefabs/Effect/PointerEffect";
    public const string DropItemPrefabPath = "Prefabs/Other/DropItem";
    public const string MonsterDeathPath = "Data/TimeLineData/怪物死亡";
    public const string BehaviorPath = "Behavior/";
    public const string sceneInfoPath = "Prefabs/Other/SceneInfo";

    public const string BGMPath = "Audio/BGM/";
    public const string BGSPath = "Audio/BGS/";
    public const string MEPath = "Audio/ME/";
    public const string SEPath = "Audio/SE/";

    public const string titlePath = "ScriptableObject/Sprites/Title";
    public const string filmDataPath = "Prefabs/FilmObj/";

    public static string gameSaveDataPath = Application.persistentDataPath;

    public const string characterPrefabPath = "Prefabs/Character";
    public const string monsterPrefabPath = "Prefabs/Monster";

    //public const string monsterSpritePath = "Prefabs/Monster/";
    public const string UIPath = "Prefabs/UI/";
}