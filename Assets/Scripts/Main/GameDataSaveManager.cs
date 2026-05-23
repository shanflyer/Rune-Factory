using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Mathematics;
using UnityEngine;
using VoxelBusters.EssentialKit;

public class GameDataSaveManager : Singleton<GameDataSaveManager>
{
    private string userName = "User";
    private UserGameSaveDataList userGameSaveDataList;

    public UserGameSaveDataList UserGameSaveDataList
    {
        get => userGameSaveDataList;
    }

    public int loadingIndex
    {
        get => _loadingIndex;
        set
        {
            _loadingIndex = value;
            userGameSaveDataList.nowSaveData = new UserGameSaveData(loadGameSaveData);
        }
    }

    public int _loadingIndex = -99;
    public UserGameSaveData UserGameSaveData
    {
        get
        {
            return userGameSaveDataList.nowSaveData;
        }
    }
    public UserGameSaveData loadGameSaveData
    {
        get
        {
            if (loadingIndex >= -1)
            {
                if (loadingIndex == -1)
                {
                    return UserGameSaveDataList.nowSaveData; 
                }
                if (loadingIndex < 3)
                {
                    var saveData = userGameSaveDataList.userGameSaveDatas[loadingIndex];
                    return saveData;
                } 
            }
             
            return null;
        }
    } 
    public void NewPlayerData()
    {
        UserGameSaveDataList.nowSaveData = new UserGameSaveData();
    }
    public bool HaveSaveFileData(int id)
    {
        return UserGameSaveData.fields.ContainsKey(id);
    }
    public async Task InitLoadSaveData()
    {
        if (loadDataIsNotNull && CharacterManager.instance.controllerCharacter == null)
        { 
            PackageManager.instance.InitFromSaveData(loadGameSaveData.packageSaveDatas);
            PackageManager.instance.playerPackages.AddRange(loadGameSaveData.otherSaveData.playerPackages);

            await CharacterManager.instance.CreatePlayer((int)loadGameSaveData.playerData.gender,loadGameSaveData.playerData.name, 0, loadGameSaveData.playerData.instanceId);

            for (var i = 0; i < loadGameSaveData.storeCounters.Count; i++)
            {
                var storeCounter = loadGameSaveData.storeCounters[i];
                storeCounter.Unpack();
                PlayerStoreManager.instance.CreatStoreCounter(storeCounter);
            }
            
            using(var e = loadGameSaveData.manufatures.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.Unpack();
                    ManufactureManager.instance.CreatManufature(e.Current);
                }
            }
            using(var e = loadGameSaveData.mapHomeEquips.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.Unpack();
                    HomeEquipManager.instance.CreatHomeEquip(e.Current);
                }
            }

            if (loadGameSaveData.otherSaveData.shortcutItems != null)
            {
                for(int i = 0; i < loadGameSaveData.otherSaveData.shortcutItems.Count; i++)
                {
                    var saveData=loadGameSaveData.otherSaveData.shortcutItems[i];
                    if (saveData.x == 0)
                    {
                        continue;
                    }
                    if (saveData.y != 0)
                    {
                        Item item = PackageManager.instance.GetItemFromInstanceId(CharacterManager.instance.controllerCharacter.instanceId, saveData.y);
                        if (item.instanceId == saveData.y)
                        {
                            SetShortcutItem setShortcutItem = new SetShortcutItem
                            {
                                characterId = CharacterManager.instance.controllerCharacter.instanceId,
                                Item = item
                            };
                            GameActionManager.instance.QueueAction(setShortcutItem);
                        } 
                    }
                    else
                    {
                        Item newItem = new Item
                        {
                            dataId = saveData.x,
                            count = PackageManager.instance.GetPackageItemCount(CharacterManager.instance.controllerCharacter.characterPackage, saveData.x)
                        }; 
                        SetShortcutItem setShortcutItem = new SetShortcutItem
                        {
                            characterId = CharacterManager.instance.controllerCharacter.instanceId,
                            Item = newItem
                        };
                        GameActionManager.instance.QueueAction(setShortcutItem);
                    }
                }
            }
            ManufactureManager.instance.InitSaveOpenFormula(loadGameSaveData.openFormulas);
             
        }
    }
    public void InitMapItemSaveData(int instanceId)
    {
        if (loadDataIsNotNull)
        {
            if (loadGameSaveData.GetChangeMapItem(instanceId, out var data))
            {
                ChangeMapItem changeMapItem = new ChangeMapItem
                {
                    itemId = instanceId,
                    newDataId = data.x,
                    animationKey = data.yz
                };
                GameActionManager.instance.QueueAction(changeMapItem);
            }

            if (loadGameSaveData.AnimationStateMapItemsDic.TryGetValue(instanceId, out var data1))
            {
                SetItemAnimation setItemAnimation = new SetItemAnimation
                {
                    id = instanceId,
                    keyX = data1.x,
                    keyY = data1.y
                };
                GameActionManager.instance.QueueAction(setItemAnimation);
            }
            loadGameSaveData.InitMapItemSaveData(instanceId);
        }
    }
    private bool loadCompleted = false;
    public void AfterInitMapLoadSaveData()
    {
        if (loadDataIsNotNull)
        {
            using(var e = loadGameSaveData.fields.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    FarmManager.instance.CreatField(e.Current);
                }
            }


            for (var i = 0; i < loadGameSaveData.shopList.Count; i++)
            {
                ShopManager.instance.InitShopList(loadGameSaveData.shopList[i]);
            }
          
            using(var e = loadGameSaveData.pastures.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.Unpack();
                    PastureManager.instance.CreatPasture(e.Current);
                }
            } 
            using(var e = loadGameSaveData.animals.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    e.Current.Unpack();
                    PastureManager.instance.CreatAnimal(e.Current);
                }
            } 
            FriendManager.instance.InitFriendSaveData(loadGameSaveData.friendSaveData);
            WeatherManager.instance.InitSaveWeather(loadGameSaveData.nowWeathers, loadGameSaveData.nextWeathers);

            loadCompleted = true;
        }
        else
        {
            loadCompleted = true;
        }
    }

    public void InitSaveDate()
    {
        if (loadDataIsNotNull && loadGameSaveData.dateData.season != Season.Default)
        {
            GameTimeManager.instance.InitSaveDate(loadGameSaveData.dateData); 
        }
      
    }
  
    public bool IsZeroGameSave
    {
        get
        {
            return string.IsNullOrEmpty(UserGameSaveData.saveTime);
        }
    }
    public void SaveSpecialItem(int2 key,int instanceId)
    {
        if (UserGameSaveData != null)
        {
            UserGameSaveData.SaveSpecialMapItem(key, instanceId);
        }
    }
    public int GetSaveMapInstance(int2 key)
    {
        if(loadGameSaveData!=null)
        {
            if (loadGameSaveData.GetSpecialMapItem(key, out var value))
            {
                return value;
            } 
        }
        return 0;
    }
   
    public int CheckMapLine(int id)
    {
        if (loadDataIsNotNull)
        { 
            if(loadGameSaveData.mapLineSaveData.TryGetValue(id,out var value))
            {
                return value;
            }
            
        }
        return -1;
    }
    public bool LoadDataSuccess { get; private set; }

    public void InitUserSaveData(string userName, string clundDataStr = null)
    {
        userGameSaveDataList = LoadUserGameSaveData(userName, clundDataStr);
    }

    private UserGameSaveDataList LoadUserGameSaveData(string userName, string clundDataStr = null)
    {
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        UserGameSaveDataList localSaveData = null;
        UserGameSaveDataList cloudSaveData = null;
        if (File.Exists(saveDataPath))
        {
            try
            {
                TryLoadSaveDataList(File.ReadAllText(saveDataPath), "本地存档", out localSaveData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Read local save failed: {saveDataPath}");
                Debug.LogException(e);
            }
        }

        if (!string.IsNullOrEmpty(clundDataStr))
        {
            TryLoadSaveDataList(clundDataStr, "云存档", out cloudSaveData);
        }

        if (localSaveData != null && cloudSaveData != null)
        {
            return SelectLatestSaveDataList(localSaveData, cloudSaveData);
        }
        return cloudSaveData ?? localSaveData ?? CreateEmptySaveDataList();
    }

    private static UserGameSaveDataList CreateEmptySaveDataList()
    {
        return new UserGameSaveDataList
        {
            nowSaveData = UserGameSaveData.CreatSaveData(-1),
            userGameSaveDatas = new List<UserGameSaveData>
            {
                UserGameSaveData.CreatSaveData(0),
                UserGameSaveData.CreatSaveData(1),
                UserGameSaveData.CreatSaveData(2)
            }
        };
    }

    private static UserGameSaveDataList SelectLatestSaveDataList(UserGameSaveDataList localSaveData, UserGameSaveDataList cloudSaveData)
    {
        var localTime = GetSaveTime(localSaveData.nowSaveData);
        var cloudTime = GetSaveTime(cloudSaveData.nowSaveData);
        return cloudTime >= localTime ? cloudSaveData : localSaveData;
    }

    private static DateTime GetSaveTime(UserGameSaveData saveData)
    {
        return DateTime.TryParse(saveData?.saveTime, out var saveTime) ? saveTime : DateTime.MinValue;
    }

    private static bool TryLoadSaveDataList(string dataStr, string source, out UserGameSaveDataList saveDataList)
    {
        if (TryDeserializeSaveDataList(dataStr, source, out saveDataList))
        {
            return true;
        }

        var decryptedDataStr = DecryptDES(dataStr);
        if (!string.Equals(decryptedDataStr, dataStr, StringComparison.Ordinal) &&
            TryDeserializeSaveDataList(decryptedDataStr, $"{source}解密数据", out saveDataList))
        {
            return true;
        }

        Debug.LogError($"Load save data failed: {source}");
        saveDataList = null;
        return false;
    }

    private static bool TryDeserializeSaveDataList(string dataStr, string source, out UserGameSaveDataList saveDataList)
    {
        try
        {
            saveDataList = JsonConvert.DeserializeObject<UserGameSaveDataList>(dataStr, JsonSerializerSettings);
            NormalizeSaveDataList(saveDataList);
            return true;
        }
        catch (Exception e)
        {
            // 候选存档必须完整初始化后才能替换当前存档，避免半加载状态污染运行时。
            Debug.LogError($"Deserialize save data failed: {source}");
            Debug.LogException(e);
            saveDataList = null;
            return false;
        }
    }

    private static void NormalizeSaveDataList(UserGameSaveDataList saveDataList)
    {
        if (saveDataList == null)
        {
            throw new InvalidDataException("Save data list is null.");
        }

        saveDataList.commonSaveData ??= new CommonSaveData();
        saveDataList.nowSaveData ??= UserGameSaveData.CreatSaveData(-1);
        saveDataList.nowSaveData.index = -1;
        saveDataList.nowSaveData.Init();

        saveDataList.userGameSaveDatas ??= new List<UserGameSaveData>();
        while (saveDataList.userGameSaveDatas.Count < 3)
        {
            saveDataList.userGameSaveDatas.Add(UserGameSaveData.CreatSaveData(saveDataList.userGameSaveDatas.Count));
        }

        for (int i = 0; i < 3; i++)
        {
            saveDataList.userGameSaveDatas[i] ??= UserGameSaveData.CreatSaveData(i);
            saveDataList.userGameSaveDatas[i].index = i;
            saveDataList.userGameSaveDatas[i].Init();
        }
    }

    public bool loadDataIsNotNull => loadGameSaveData != null && loadGameSaveData.dateData.season != Season.Default;
    public CharacterSaveData GetCharacterSaveData(int dataId)
    {
        if (loadDataIsNotNull)
        {
            if(loadGameSaveData.characterSaveDatas.TryGetValue(dataId,out var characterSaveData))
            {
                characterSaveData.Unpack();
                return characterSaveData;
            }
            else if(loadGameSaveData.playerData.dataId==dataId)
            {
                loadGameSaveData.playerData.Unpack();
                return loadGameSaveData.playerData;
            } 
        }
        return null;
    }
    public async void InitPlayerData(string playerName, Gender gender, Season season, int day, int year = 1300)
    {
        UserGameSaveDataList.nowSaveData = new UserGameSaveData()
        {
            index = -1
        };
        UserGameSaveData.playerData = new CharacterSaveData();
        UserGameSaveData.playerData.name = playerName;
        UserGameSaveData.playerData.gender = gender;
        UserGameSaveData.playerData.brithDay = new BrithDay
        {
            year = year,
            season = season,
            day = day
        };
        UserGameSaveData.playerData.dataId = (int)gender;
        UserGameSaveData.playerData.Pack();
        //NPCManager.instance.CreatZeroNPC();
       await CharacterManager.instance.CreatePlayer((int)gender,playerName, 0);
    }
    static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.None
    };

    static GameDataSaveManager()
    {
        GameJsonSettings.AddGameConverters(JsonSerializerSettings);
    }

    public static string ObjToString<T>(T t)
    {
        return JsonConvert.SerializeObject(t, JsonSerializerSettings);
    }
    public static T StringToObj<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str, JsonSerializerSettings);
    }

    private void SaveUserGameSaveData()
    {
        UserGameSaveData.otherSaveData.uid = MyInstance.instance.MaxUid;

        //包裹数据
        var packageSaveDatas = PackageManager.instance.GetPackageSaveData();
        UserGameSaveData.packageSaveDatas = packageSaveDatas;
        UserGameSaveData.otherSaveData.playerPackages = PackageManager.instance.playerPackages;
        //npc数据
        var characters = CharacterManager.instance.GetAllCharacters();
        UserGameSaveData.characterSaveDatas.Clear();
        for (int i=0;i<characters.Count; i++)
        {
            if (characters[i].characterData.id != UserGameSaveData.playerData.dataId)
            {
                CharacterSaveData characterSaveData = new CharacterSaveData(characters[i]);
                UserGameSaveData.characterSaveDatas[characterSaveData.dataId]=characterSaveData;
            }
            else
            {
                UserGameSaveData.playerData.SetCharacter(characters[i], UserGameSaveData.playerData.name); 
            }
        }
        //玩家数据
        UserGameSaveData.otherSaveData.playerPackages = PackageManager.instance.playerPackages;

        //时间
        UserGameSaveData.dateData = new GameDateSaveData
        {
            year = GameTimeManager.instance.Year,
            day = GameTimeManager.instance.Day,
            season = GameTimeManager.instance.Season,
            minute = GameTimeManager.instance.Minute,
            hour = GameTimeManager.instance.Hour,
            week=GameTimeManager.instance.Week
        };

        UserGameSaveData.otherSaveData.shortcutItems = new List<int2>();
        var package= ShortcutManager.instance.playerShortcutPackage;
        if (package != null)
        {
            for (var i = 0; i < package.items.Length; i++)
                UserGameSaveData.otherSaveData.shortcutItems.Add(new int2(package.items[i].dataId,
                    package.items[i].instanceId));
        }
       

        //玩家商店
        UserGameSaveData.storeCounters.Clear();
        if (PlayerStoreManager.instance != null && PlayerStoreManager.instance.RuntimeStoreCounters != null)
            foreach (var runtimeStoreCounter in PlayerStoreManager.instance.RuntimeStoreCounters.Values)
                UserGameSaveData.storeCounters.Add(new StoreCounterSaveData(runtimeStoreCounter));


        //友情关系
        if (FriendManager.instance != null)
        UserGameSaveData.friendSaveData = FriendManager.instance.GetFriendSaveData();
        UserGameSaveData.otherSaveData.gold = PayManager.instance.NowGold;
        UserGameSaveDataList.commonSaveData.diamond = PayManager.instance.NowDiamond;
        UserGameSaveData.SaveData();
    }
    public void RefreshUserCommonSaveData(int diamond)
    {
        UserGameSaveDataList.commonSaveData.diamond = diamond;
        CloudServices.SetInt("diamond", userGameSaveDataList.commonSaveData.diamond);
    }
    
    public bool SetFishSaveData(int fish,int length,int place)
    {
        var saveData = UserGameSaveData;
        var fishDatas = saveData.fishSaveDatas;
          
        if(fishDatas.TryGetValue(fish,out var fishSaveData))
        {
            bool newRecord = fishSaveData.length< length;

            fishSaveData.length = length;
            fishSaveData.dataId = fish;
            if (!fishSaveData.places.Contains(place))
            {
                fishSaveData.places.Add(place);
            }
            return newRecord;
        }
        else
        {
            fishSaveData = new FishSaveData();
            
            fishSaveData.length = length;
            fishSaveData.dataId = fish;
            fishSaveData.places = new List<int> { place };
            fishDatas.Add(fish,fishSaveData);

            return true;
        } 
    }
   
    public FishSaveData GetFishDataSave(int id)
    {
        FishSaveData fishSaveData=null;
        var fishDatas = UserGameSaveData.fishSaveDatas;
        if (fishDatas != null)
        {
            fishDatas.TryGetValue(id, out fishSaveData);
        }
        return fishSaveData;
    }
    public bool GetPlantFruitCount(int id,out int count)
    {
        if(UserGameSaveData.plantSaveDatas.TryGetValue(id,out var plantSaveData))
        {
            count = plantSaveData.fruitCount;
            return true;
        }
        count = 0;
        return false;
    }
    public async void SetPlantFruitCount(int id,int count)
    {
        if (!UserGameSaveData.plantSaveDatas.TryGetValue(id, out var plantSaveData))
        {
            plantSaveData = new PlantSaveData
            {
                dataId = id,
                fruitCount = count
            };
            UserGameSaveData.plantSaveDatas.Add(id, plantSaveData);

            PlantData plantData = await GameDataManager.instance.GetAsyncData<PlantData>(id);

            ItemResultInfo itemResultInfo = new ItemResultInfo
            {
                icon = plantData.icon,
                info0 = plantData.plantName,
                info1="发现了新农作物！"
            };
            GameNotificationManager.instance.ShowItemResultInfo(itemResultInfo);
        }
        plantSaveData.fruitCount += count;
    }
  
    List<FieldInfo> UserGameSaveDataIntFields;
    List<FieldInfo> UserGameSaveDataStringFields;
    List<FieldInfo> UserGameSaveDataJsonFields;
    public override void Init()
    {
        base.Init();
        //InitUserSaveData();
        EnsureSaveFieldCaches();
    }

    private void EnsureSaveFieldCaches()
    {
        if (UserGameSaveDataIntFields != null && UserGameSaveDataStringFields != null && UserGameSaveDataJsonFields != null)
        {
            return;
        }

        Type type = typeof(UserGameSaveData);
        var UserGameSaveDataFields=type.GetFields();
        UserGameSaveDataIntFields = new List<FieldInfo>();
        UserGameSaveDataStringFields = new List<FieldInfo>();
        UserGameSaveDataJsonFields = new List<FieldInfo>();

        Type intType = typeof(int);
        Type strType=typeof(string);
        for (int i = 0; i < UserGameSaveDataFields.Length; i++)
        {
            var field = UserGameSaveDataFields[i]; 
            if (field.FieldType == intType)
            {
                UserGameSaveDataIntFields.Add(field);
            }else if (field.FieldType == strType)
            {
                UserGameSaveDataStringFields.Add(field);
            }
            else
            {
                UserGameSaveDataJsonFields.Add(field);
            }
        }
    }

    protected override void Clear()
    {
        base.Clear();
    }

    public void TryAutoSaveData()
    {
       // if (loadCompleted&&GameGuideManager.instance.endGuideFilmIndex >= 0&&!GameController.instance.hideSave)
        {
            SaveData(-1);
        }
    }

    public bool SaveData(int selectSaveIndex)
    {
        if (selectSaveIndex > 2)
        {
            return false;
        }
        EnsureSaveFieldCaches();
        var nowSaveDataBackup = UserGameSaveData != null ? new UserGameSaveData(UserGameSaveData) : null;
        var selectedSaveDataBackup = selectSaveIndex >= 0 && userGameSaveDataList.userGameSaveDatas[selectSaveIndex] != null
            ? new UserGameSaveData(userGameSaveDataList.userGameSaveDatas[selectSaveIndex])
            : null;
        try
        {
            SaveUserGameSaveData();

            if (selectSaveIndex >= 0)
            {
                userGameSaveDataList.userGameSaveDatas[selectSaveIndex] =new UserGameSaveData(UserGameSaveData);
                userGameSaveDataList.userGameSaveDatas[selectSaveIndex].index = selectSaveIndex;
            }

            SaveCloudData(selectSaveIndex);
            CloudServices.Synchronize();
        }
        catch (Exception e)
        {
            // 保存过程中任何一步失败都回滚内存快照，避免 UI 显示已保存但实际云端未落盘。
            Debug.LogError($"Save data failed. index={selectSaveIndex}");
            Debug.LogException(e);
            if (nowSaveDataBackup != null)
            {
                userGameSaveDataList.nowSaveData = nowSaveDataBackup;
            }
            if (selectSaveIndex >= 0)
            {
                userGameSaveDataList.userGameSaveDatas[selectSaveIndex] = selectedSaveDataBackup;
            }
            return false;
        }
        /*

#if UNITY_EDITOR
        string strs = JsonConvert.SerializeObject(userGameSaveDataList, JsonSerializerSettings);
        if (GameDataManager.instance.GlobalData.Encrypt)
        {
            strs = EncryptDES(strs);
        } 
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        File.WriteAllText(saveDataPath, strs);
#elif UNITY_ANDROID || UNITY_IOS
      SaveCloudData(selectSaveIndex);
#endif */
        return true;
    }

    public void DeleteSaveData(UserGameSaveData userGameSaveData)
    {
        if (userGameSaveData == userGameSaveDataList.nowSaveData)
        {

        }
        else
        {
            for (int i = 0; i < userGameSaveDataList.userGameSaveDatas.Count; i++)
            {
                var saveData = userGameSaveDataList.userGameSaveDatas[i];
                if (saveData==userGameSaveData)
                {
                    userGameSaveDataList.userGameSaveDatas[i] = null;

                    ClearCloudData($"player_{i}");
                    break;
                }
            }
        }
    }

    public bool CopySaveData(UserGameSaveData userGameSaveData)
    {
        for(int i=0;i< userGameSaveDataList.userGameSaveDatas.Count; i++)
        {
            var saveData = userGameSaveDataList.userGameSaveDatas[i];
            if (saveData == null || string.IsNullOrEmpty(saveData.saveTime))
            {
                userGameSaveDataList.userGameSaveDatas[i] = new UserGameSaveData(userGameSaveData);
                SetCloudData(userGameSaveDataList.userGameSaveDatas[i], $"player_{i}");
                return true;
            }
        }

        return false;
    }

    private static string Mykey = "i1jI0Ooz";
    private static byte[] Keys = { 0x00, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

    public static string EncryptDES(string encryptString)
    {
        return EncryptDES(encryptString, Mykey);
    }

    public static string DecryptDES(string decryptString)
    {
        return DecryptDES(decryptString, Mykey);
    }

    /// <summary>
    /// DES加密字符串
    /// </summary>
    /// <param name="encryptString">待加密的字符串</param>
    /// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
    public static string EncryptDES(string encryptString, string encryptKey)
    {
        try
        {
            byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
            byte[] rgbIV = Keys;
            byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
            DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            cStream.Close();
            return Convert.ToBase64String(mStream.ToArray());
        }
        catch
        {
            return encryptString;
        }
    }

    /// <summary>
    /// DES解密字符串
    /// </summary>
    /// <param name="decryptString">待解密的字符串</param>
    /// <returns>解密成功返回解密后的字符串，失败返源串</returns>
    public static string DecryptDES(string decryptString, string decryptKey)
    {
        try
        {
            byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey);
            byte[] rgbIV = Keys;
            byte[] inputByteArray = Convert.FromBase64String(decryptString);
            DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
            MemoryStream mStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(mStream, DCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
            cStream.Write(inputByteArray, 0, inputByteArray.Length);
            cStream.FlushFinalBlock();
            cStream.Close();
            return Encoding.UTF8.GetString(mStream.ToArray());
        }
        catch
        {
            Debug.Log("catch");
            return decryptString;
        }
    }


    void SaveCloudData(int index=-1)
    {
        CloudServices.SetInt("diamond", userGameSaveDataList.commonSaveData.diamond);
        if (index < 0)
        {
            SetCloudData(userGameSaveDataList.nowSaveData, "auto_");
        }
        else if(index<3)
        {
            SetCloudData(userGameSaveDataList.userGameSaveDatas[index], $"player_{index}");
        }
    }

    private void ClearCloudData(string keyStr)
    {
        for (var i = 0; i < UserGameSaveDataIntFields.Count; i++)
        {
            var field = UserGameSaveDataIntFields[i];
            var key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.RemoveKey(key);
        }

        for (var i = 0; i < UserGameSaveDataStringFields.Count; i++)
        {
            var field = UserGameSaveDataStringFields[i];
            var key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.RemoveKey(key);
        }

        for (var i = 0; i < UserGameSaveDataJsonFields.Count; i++)
        {
            var field = UserGameSaveDataJsonFields[i];
            var key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.RemoveKey(key);
        }
        //CloudServices.RemoveKey(GameCommon.BlendString(keyStr, "specialMapItemList"));
    }
    void SetCloudData(UserGameSaveData nowSaveData, string keyStr)
    {
        for (int i = 0; i < UserGameSaveDataIntFields.Count; i++)
        {
            var field = UserGameSaveDataIntFields[i];
            int value = (int)field.GetValue(nowSaveData);
            string key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.SetInt(key, value);
        }
        for (int i = 0; i < UserGameSaveDataStringFields.Count; i++)
        {
            var field = UserGameSaveDataStringFields[i];
            string value = (string)field.GetValue(nowSaveData);
            string key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.SetString(key, value);
        }
        for (int i = 0; i < UserGameSaveDataJsonFields.Count; i++)
        {
            var field = UserGameSaveDataJsonFields[i];
            var obj = field.GetValue(nowSaveData);
            var objStr = JsonConvert.SerializeObject(obj, JsonSerializerSettings);
            string key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.SetString(key, objStr);
        }
        //CloudServices.RemoveKey(GameCommon.BlendString(keyStr, "specialMapItemList"));
    }
    public void LoadCloudData()
    {
        EnsureSaveFieldCaches();
        var previousSaveDataList = userGameSaveDataList;
        try
        {
            var loadedSaveDataList = LoadCloudDataList();
            // 云存档先完整构建并初始化，成功后再替换当前引用，避免半加载状态进入游戏。
            userGameSaveDataList = loadedSaveDataList;
            LoadDataSuccess = true;
            loadCompleted = HasAnySaveData(userGameSaveDataList);
        }
        catch (Exception e)
        {
            Debug.LogError("Load cloud save data failed.");
            Debug.LogException(e);
            LoadDataSuccess = false;
            userGameSaveDataList = previousSaveDataList ?? CreateEmptySaveDataList();
            loadCompleted = HasAnySaveData(userGameSaveDataList);
        }
    }

    private UserGameSaveDataList LoadCloudDataList()
    {
        var loadedSaveDataList = new UserGameSaveDataList();
        int diamond = CloudServices.GetInt("diamond");
        loadedSaveDataList.commonSaveData = new CommonSaveData
        {
            diamond = diamond
        };
        loadedSaveDataList.nowSaveData = GameController.instance.startPlay ? new UserGameSaveData() : LoadUserData("auto_");
        loadedSaveDataList.userGameSaveDatas = new List<UserGameSaveData>
        {
            LoadUserData("player_0"),
            LoadUserData("player_1"),
            LoadUserData("player_2")
        };

        NormalizeSaveDataList(loadedSaveDataList);
        return loadedSaveDataList;

        UserGameSaveData LoadUserData(string key)
        {
           // CloudServices.RemoveKey(GameCommon.BlendString(key, "specialMapItemList"));
            UserGameSaveData userData = new UserGameSaveData();
            for(int i = 0; i < UserGameSaveDataIntFields.Count; i++)
            {
                var field = UserGameSaveDataIntFields[i];
                string fieldKey = $"{key}{field.Name}";
                int value= CloudServices.GetInt(fieldKey);
                field.SetValue(userData, value);
            }
            for(int i = 0; i < UserGameSaveDataStringFields.Count; i++)
            {
                var field = UserGameSaveDataStringFields[i];
                string fieldKey = $"{key}{field.Name}";
                string value = CloudServices.GetString(fieldKey);
                field.SetValue(userData, value);
            }
            for(int i = 0; i < UserGameSaveDataJsonFields.Count; i++)
            {
                var field = UserGameSaveDataJsonFields[i];
                string fieldKey = $"{key}{field.Name}";
                string value = CloudServices.GetString(fieldKey);
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                      
                        object obj = JsonConvert.DeserializeObject(value, field.FieldType, JsonSerializerSettings);
                        field.SetValue(userData, obj);
                    }
                    catch
                    {
                        Debug.LogError($"DeserializeObject{field.Name}-type{field.FieldType}-value:{value}");
                    }
                   
                }
                else
                {

                }                
            }
            return userData;
        }
    }

    private static bool HasAnySaveData(UserGameSaveDataList saveDataList)
    {
        return !string.IsNullOrEmpty(saveDataList.nowSaveData?.saveTime) ||
               saveDataList.userGameSaveDatas.Count > 0 && !string.IsNullOrEmpty(saveDataList.userGameSaveDatas[0]?.saveTime) ||
               saveDataList.userGameSaveDatas.Count > 1 && !string.IsNullOrEmpty(saveDataList.userGameSaveDatas[1]?.saveTime) ||
               saveDataList.userGameSaveDatas.Count > 2 && !string.IsNullOrEmpty(saveDataList.userGameSaveDatas[2]?.saveTime);
    }

}
