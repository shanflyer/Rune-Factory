using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
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

    public int loadingIndex = -99;
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
                    return   userGameSaveDataList.userGameSaveDatas[loadingIndex];
                } 
            }
            return null;
        }
    } 

    public bool HaveSaveFileData(int id)
    {
        return UserGameSaveData.fields.ContainsKey(id);
    }
    public async Task InitLoadSaveData()
    {
        if (loadGameSaveData != null&&CharacterManager.instance.controllerCharacter==null)
        { 
            PackageManager.instance.InitFromSaveData(loadGameSaveData.packageSaveDatas);
            PackageManager.instance.playerPackages.AddRange(loadGameSaveData.otherSaveData.playerPackages);

            await CharacterManager.instance.CreatePlayer((int)loadGameSaveData.playerData.gender, 0, loadGameSaveData.playerData.instanceId);

            using(var e = loadGameSaveData.storeCounters.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    PlayerStoreManager.instance.CreatStoreCounter(e.Current);
                }
            } 
            using(var e = loadGameSaveData.manufatures.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    ManufactureManager.instance.CreatManufature(e.Current);
                }
            }
            using(var e = loadGameSaveData.mapHomeEquips.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
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
        if (loadGameSaveData != null)
        {
            if (loadGameSaveData.changeMapItems.TryGetValue(instanceId, out var data))
            {
                ChangeMapItem changeMapItem = new ChangeMapItem
                {
                    itemId = instanceId,
                    newDataId = data.x,
                    animationKey = data.yz
                };
                GameActionManager.instance.QueueAction(changeMapItem);
            }
            if (loadGameSaveData.SetAnimationStateMapItems.TryGetValue(instanceId, out var data1))
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
        if (loadGameSaveData != null)
        {
            using(var e = loadGameSaveData.fields.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    FarmManager.instance.CreatField(e.Current);
                }
            } 
            using(var e = loadGameSaveData.shops.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    ShopManager.instance.InitShop(e.Current);
                }
            }
            using(var e = loadGameSaveData.shopLists.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    ShopManager.instance.InitShopList(e.Current);
                }
            }
            using(var e = loadGameSaveData.pastures.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    PastureManager.instance.CreatPasture(e.Current);
                }
            } 
            using(var e = loadGameSaveData.animals.Values.GetEnumerator())
            {
                while (e.MoveNext())
                {
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
        if (loadGameSaveData != null)
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
    public void InitMapInstanceData()
    {
        if (loadGameSaveData!=null)
        { 
            for(int i=0;i<loadGameSaveData.specialMapItemList.Count;i++)
            {
                MyInstance.instance.AddInstance(loadGameSaveData.specialMapItemList[i].z);
            } 
        }
    }
    public int CheckMapLine(int id)
    {
        if (loadGameSaveData != null)
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
        if (File.Exists(saveDataPath))
        {
            LoadDataSuccess = true;
            string dataStr = File.ReadAllText(saveDataPath);
            UserGameSaveDataList userGameSaveDataList = null;
            try
            {
                userGameSaveDataList = JsonConvert.DeserializeObject<UserGameSaveDataList>(dataStr);
            }
            catch
            {
                dataStr = DecryptDES(dataStr);
                userGameSaveDataList = JsonConvert.DeserializeObject<UserGameSaveDataList>(dataStr);
            }
            //
            userGameSaveDataList.nowSaveData.Init();
            userGameSaveDataList.nowSaveData.index = -1;
            for (int i = 0; i < userGameSaveDataList.userGameSaveDatas.Count; i++)
            {
                userGameSaveDataList.userGameSaveDatas[i].Init();
                userGameSaveDataList.userGameSaveDatas[i].index = i;
            }
            if (string.IsNullOrEmpty(clundDataStr))
            {
                return userGameSaveDataList;
            }
            UserGameSaveDataList userGameSaveDataList2 = null;
            try
            {
                userGameSaveDataList2 = JsonConvert.DeserializeObject<UserGameSaveDataList>(clundDataStr);
            }
            catch
            {
                clundDataStr = DecryptDES(clundDataStr);
                userGameSaveDataList2 = JsonConvert.DeserializeObject<UserGameSaveDataList>(clundDataStr);
            }
            //
            userGameSaveDataList2.nowSaveData.Init();
            userGameSaveDataList2.nowSaveData.index = -1;
            for (int i = 0; i < userGameSaveDataList2.userGameSaveDatas.Count; i++)
            {
                userGameSaveDataList2.userGameSaveDatas[i].Init();
                userGameSaveDataList2.userGameSaveDatas[i].index = i;
            }

            DateTime t0 = Convert.ToDateTime(userGameSaveDataList.nowSaveData.saveTime);
            DateTime t2 = Convert.ToDateTime(userGameSaveDataList2.nowSaveData.saveTime);
            if (t2 >= t0)
            {
                return userGameSaveDataList2;
            }
            else
            {
                return userGameSaveDataList;
            }


        }
        else
        {
            if (string.IsNullOrEmpty(clundDataStr))
            {
                UserGameSaveDataList userGameSaveDataList = new UserGameSaveDataList();
                userGameSaveDataList.nowSaveData = UserGameSaveData.CreatSaveData(-1);
                userGameSaveDataList.userGameSaveDatas = new List<UserGameSaveData>
                {
                UserGameSaveData.CreatSaveData(0),UserGameSaveData.CreatSaveData(1),UserGameSaveData.CreatSaveData(2)
                 };
                return userGameSaveDataList;
            }
            else
            {
                UserGameSaveDataList userGameSaveDataList2 = null;
                try
                {
                    userGameSaveDataList2 = JsonConvert.DeserializeObject<UserGameSaveDataList>(clundDataStr);
                }
                catch
                {
                    clundDataStr = DecryptDES(clundDataStr);
                    userGameSaveDataList2 = JsonConvert.DeserializeObject<UserGameSaveDataList>(clundDataStr);
                }
                //
                userGameSaveDataList2.nowSaveData.Init();
                userGameSaveDataList2.nowSaveData.index = -1;
                for (int i = 0; i < userGameSaveDataList2.userGameSaveDatas.Count; i++)
                {
                    userGameSaveDataList2.userGameSaveDatas[i].Init();
                    userGameSaveDataList2.userGameSaveDatas[i].index = i;
                }
                return userGameSaveDataList2;
            }

        }
    } 
     
    public CharacterSaveData GetCharacterSaveData(int dataId)
    {
        if (loadGameSaveData != null)
        {
            if(loadGameSaveData.characterSaveDatas.TryGetValue(dataId,out var characterSaveData))
            {
                return characterSaveData;
            }
            else if(loadGameSaveData.playerData.dataId==dataId)
            {
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
        //NPCManager.instance.CreatZeroNPC();
       await CharacterManager.instance.CreatePlayer((int)gender, 0);
    }
    static JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
        MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore,
        DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
        TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
        Formatting = Newtonsoft.Json.Formatting.None,
    };

    public static string ObjToString<T>(T t)
    {
        return JsonConvert.SerializeObject(t, JsonSerializerSettings);
    }
    public static T StringToObj<T>(string str)
    {
        return JsonConvert.DeserializeObject<T>(str);
    }

    private void SaveUserGameSaveData()
    {
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
         for(int i=0;i< package.items.Length; i++)
        {
            UserGameSaveData.otherSaveData.shortcutItems.Add(new int2(package.items[i].dataId, package.items[i].instanceId));
        }
        //友情关系
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

  
    List<FieldInfo> UserGameSaveDataIntFields;
    List<FieldInfo> UserGameSaveDataStringFields;
    List<FieldInfo> UserGameSaveDataJsonFields;
    public override void Init()
    {
        base.Init();
        //InitUserSaveData();

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
        SaveUserGameSaveData();

        if (selectSaveIndex >= 0)
        { 
            userGameSaveDataList.userGameSaveDatas[selectSaveIndex] =new UserGameSaveData(UserGameSaveData);
            userGameSaveDataList.userGameSaveDatas[selectSaveIndex].index = selectSaveIndex;
        }
        
         SaveCloudData(selectSaveIndex);
        CloudServices.Synchronize();
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
                    userGameSaveDataList.userGameSaveDatas[i] = new UserGameSaveData();

                    SetCloudData(userGameSaveDataList.userGameSaveDatas[i], $"player_{i}");
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
            if (string.IsNullOrEmpty(saveData.saveTime))
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
            var objStr = JsonConvert.SerializeObject(obj);
            string key = GameCommon.BlendString(keyStr, field.Name);
            CloudServices.SetString(key, objStr);
        }
        CloudServices.RemoveKey(GameCommon.BlendString(keyStr, "specialMapItemList"));
    }
    public void LoadCloudData()
    { 
        userGameSaveDataList = new UserGameSaveDataList();
        int diamond = CloudServices.GetInt("diamond");
        userGameSaveDataList.commonSaveData = new CommonSaveData
        {
            diamond = diamond
        };
        userGameSaveDataList.nowSaveData = LoadUserData("auto_");
        userGameSaveDataList.nowSaveData.index=-1;
        userGameSaveDataList.nowSaveData.Init();
        userGameSaveDataList.userGameSaveDatas = new List<UserGameSaveData>();

        var nowSaveData0 = LoadUserData("player_0");
        var nowSaveData1 = LoadUserData("player_1");
        var nowSaveData2 = LoadUserData("player_2");
        nowSaveData0.Init();
        nowSaveData1.Init();
        nowSaveData2.Init();
        nowSaveData0.index = 0;
        nowSaveData1.index = 1;
        nowSaveData2.index = 2;

        userGameSaveDataList.userGameSaveDatas.Add(nowSaveData0);
        userGameSaveDataList.userGameSaveDatas.Add(nowSaveData1);
        userGameSaveDataList.userGameSaveDatas.Add(nowSaveData2);

        UserGameSaveData LoadUserData(string key)
        {
            CloudServices.RemoveKey(GameCommon.BlendString(key, "specialMapItemList"));
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
                      
                        object obj = JsonConvert.DeserializeObject(value, field.FieldType);
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

        loadCompleted = !string.IsNullOrEmpty(userGameSaveDataList.nowSaveData.saveTime) || !string.IsNullOrEmpty(userGameSaveDataList.userGameSaveDatas[0].saveTime) ||
            !string.IsNullOrEmpty(userGameSaveDataList.userGameSaveDatas[1].saveTime) || !string.IsNullOrEmpty(userGameSaveDataList.userGameSaveDatas[2].saveTime);
    }

    
}