using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

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
                if (_loadGameSaveData == null)
                {
                    if (loadingIndex == -1)
                    {
                        _loadGameSaveData=UserGameSaveDataList.nowSaveData;
                    }
                    if (loadingIndex < 3)
                    {
                        _loadGameSaveData=userGameSaveDataList.userGameSaveDatas[loadingIndex];
                    }
                }
                return _loadGameSaveData;
            }
            return null;
        }
    }
    private UserGameSaveData _loadGameSaveData;

    public async Task InitLoadSaveData()
    {
        if (loadGameSaveData != null)
        { 
            PackageManager.instance.InitFromSaveData(loadGameSaveData.packageSaveDatas);

            await CharacterManager.instance.CreatPlayer((int)loadGameSaveData.playerData.gender, 0, loadGameSaveData.playerData.instanceId);

            foreach (var data in loadGameSaveData.storeCounters)
            {
                PlayerStoreManager.instance.CreatStoreCounter(data.Value);
            }

            foreach(var data in loadGameSaveData.manufatures)
            {
                ManufatureManager.instance.CreatManufature(data.Value);
            }

            foreach(var data in loadGameSaveData.mapHomeEquips)
            {
                HomeEquipManager.instance.CreatHomeEquip(data.Value);
            }
        }
    }
    public void AfterInitMapLoadSaveData()
    {
        if (loadGameSaveData != null)
        {
            foreach(var data in loadGameSaveData.fields)
            {
                FarmManager.instance.CreatField(data.Value);
            }

            foreach (var data in loadGameSaveData.changeMapItems)
            {
                ChangeMapItem changeMapItem = new ChangeMapItem
                {
                    itemId = data.Key,
                    newDataId = data.Value.x,
                    animationKey = data.Value.yz
                };
                GameActionManager.instance.QueueAction(changeMapItem);
            }

            foreach(var data in loadGameSaveData.SetAnimationStateMapItems)
            {
                SetItemAnimation setItemAnimation = new SetItemAnimation
                {
                    id = data.Key,
                    keyX = data.Value.x,
                    keyY = data.Value.y
                };
                GameActionManager.instance.QueueAction(setItemAnimation);
            }

            foreach(var data in loadGameSaveData.removeMapItemOperates)
            {
                RemoveMapItemOperate removeMapItemOperate = new RemoveMapItemOperate
                {
                    mapItemId = data.x,
                    removeOperateId = data.y
                };
                GameActionManager.instance.QueueAction(removeMapItemOperate);
            }

            foreach (var data in loadGameSaveData.addMapItemOperates)
            {
                AddMapItemOperate addMapItemOperate = new AddMapItemOperate
                {
                    mapItemId = data.x,
                    addeOperateId = data.y
                };
                GameActionManager.instance.QueueAction(addMapItemOperate);
            }

            foreach(var data in loadGameSaveData.RemoveMapItemCollider)
            {
                RemoveMapItemCollider removeMapItemCollider = new RemoveMapItemCollider
                {
                    mapItemInstanceId = data
                };
                GameActionManager.instance.QueueAction(removeMapItemCollider);
            }

            foreach(var data in loadGameSaveData.pastures)
            {
                PastureManager.instance.CreatPasture(data.Value);
            }
            foreach(var data in loadGameSaveData.animals)
            {
                PastureManager.instance.CreatAnimal(data.Value);
            }

            FriendManager.instance.InitFriendSaveData(loadGameSaveData.friendSaveData);
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

    public int GetSaveMapInstance(int2 key)
    {
        if(loadGameSaveData!=null)
        {
            if (loadGameSaveData.specialMapItem.TryGetValue(key, out var value))
            {
                return value;
            }
        }
        return 0;
    }
    public void InitMapInstanceData(MyInstance myInstance)
    {
        if (loadGameSaveData!=null)
        { 
            foreach (var d in loadGameSaveData.specialMapItem)
            {
                myInstance.AddInstance(d.Value);
            } 
        }
    }
    public bool CheckMapLine(int id)
    {
        if (loadGameSaveData != null)
        { 
            if(loadGameSaveData.mapLineSaveData.TryGetValue(id,out var value))
            {
                return value == 1;
            }
            
        }
        return true;
    }

    private void InitUserSaveData()
    {
        userGameSaveDataList = LoadUserGameSaveData(userName);
    }
    public bool LoadDataSuccess { get; private set; }
    public UserGameSaveDataList LoadUserGameSaveData(string userName)
    {
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        if (File.Exists(saveDataPath))
        {
            LoadDataSuccess = true;
            string dataStr =File.ReadAllText(saveDataPath);
           //dataStr = DecryptDES(dataStr);
            UserGameSaveDataList userGameSaveDataList = JsonConvert.DeserializeObject<UserGameSaveDataList>(dataStr);
            userGameSaveDataList.nowSaveData.Init();
            for(int i = 0; i < userGameSaveDataList.userGameSaveDatas.Count; i++)
            {
                userGameSaveDataList.userGameSaveDatas[i].Init();
            }
            return userGameSaveDataList;
        }
        else
        {
            UserGameSaveDataList userGameSaveDataList =new UserGameSaveDataList();
            userGameSaveDataList.nowSaveData = UserGameSaveData.CreatSaveData(-1);
            userGameSaveDataList.userGameSaveDatas = new List<UserGameSaveData>
            {
                UserGameSaveData.CreatSaveData(0),UserGameSaveData.CreatSaveData(1),UserGameSaveData.CreatSaveData(2)
            };
            return userGameSaveDataList;
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
    public void InitPlayerData(string playerName, Gender gender, Season season, int day, int year = 1300)
    {
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
        CharacterManager.instance.CreatPlayer((int)gender, 0);
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
    private void SaveUserGameSaveData()
    {
        //包裹数据
        var packageSaveDatas = PackageManager.instance.GetPackageSaveData();
        UserGameSaveData.packageSaveDatas = packageSaveDatas;

        //npc数据
        var characters = CharacterManager.instance.GetAllCharacters();
        UserGameSaveData.characterSaveDatas.Clear();
        for (int i=0;i<characters.Count; i++)
        {
            if (characters[i].characterData.id != UserGameSaveData.playerData.dataId)
            {
                CharacterSaveData characterSaveData = new CharacterSaveData(characters[i]);
                UserGameSaveData.characterSaveDatas.Add(characterSaveData.dataId,characterSaveData);
            }
            else
            {
                UserGameSaveData.playerData = new CharacterSaveData(characters[i]);
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

        //友情关系
        UserGameSaveData.friendSaveData = FriendManager.instance.GetFriendSaveData();

        UserGameSaveData.SaveData();
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


    public override void Init()
    {
        base.Init();
        InitUserSaveData();
    }

    protected override void Clear()
    {
        base.Clear();
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
        }
         

        string strs = JsonConvert.SerializeObject(userGameSaveDataList, JsonSerializerSettings);
        //strs = EncryptDES(strs);
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        File.WriteAllText(saveDataPath, strs); 
        return true;
    }

    public void DeletaSaveData(UserGameSaveData userGameSaveData)
    { }

    public bool CopySaveData(UserGameSaveData userGameSaveData)
    {
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
}