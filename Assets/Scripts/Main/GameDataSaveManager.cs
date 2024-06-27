using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameDataSaveManager : Singleton<GameDataSaveManager>
{
    private string userName = "User";
    private UserGameSaveDataList userGameSaveDataList;

    public UserGameSaveDataList UserGameSaveDataList
    {
        get => userGameSaveDataList;
    }

    private int selectSaveIndex;

    public UserGameSaveData UserGameSaveData
    {
        get
        {
            if (selectSaveIndex < 0)
            {
                return userGameSaveDataList.nowSaveData;
            }
            return userGameSaveDataList.userGameSaveDatas[selectSaveIndex];
        }
    }

    public bool IsZeroGameSave
    {
        get
        {
            return UserGameSaveData.saveTime == "1989";
        }
    }

    private async void InitUserSaveData()
    {
        userGameSaveDataList = await LoadUserGameSaveData(userName);
    }

    public async Task<UserGameSaveDataList> LoadUserGameSaveData(string userName)
    {
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        if (File.Exists(saveDataPath))
        {
            string dataStr = await File.ReadAllTextAsync(saveDataPath);
            string _dataStr = DecryptDES(dataStr);
            try
            {
                UserGameSaveDataList userGameSaveDataList = JsonConvert.DeserializeObject<UserGameSaveDataList>(_dataStr);
                return userGameSaveDataList;
            }
            catch
            {
                UserGameSaveDataList userGameSaveDataList = JsonConvert.DeserializeObject<UserGameSaveDataList>(dataStr);
                return userGameSaveDataList;
            }
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

    public void InitPlayerData(string playerName, Gender gender, Season season, int day, int year = 1300)
    {
        if (UserGameSaveDataList.userGameSaveDatas.Count > selectSaveIndex)
        {
            var userGameSaveData = UserGameSaveDataList.userGameSaveDatas[selectSaveIndex];

            userGameSaveData.playerData.name = playerName;
            userGameSaveData.playerData.gender = gender;
            userGameSaveData.playerData.brithDay = new BrithDay
            {
                year = year,
                season = season,
                day = day
            };
            UserGameSaveDataList.userGameSaveDatas[selectSaveIndex] = userGameSaveData;
        }
        else
        {
            var userGameSaveData = UserGameSaveData.CreatSaveData(selectSaveIndex);
            UserGameSaveDataList.userGameSaveDatas.Add(userGameSaveData);
        }

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
        var packageSaveDatas = PackageManager.instance.GetPackageSaveData();

        if (selectSaveIndex < 0)
        {
            userGameSaveDataList.nowSaveData.packageSaveDatas = packageSaveDatas;
        }
        else
        {
            var userGameSaveData = userGameSaveDataList.userGameSaveDatas[selectSaveIndex];
            userGameSaveData.packageSaveDatas = packageSaveDatas;
            userGameSaveDataList.userGameSaveDatas[selectSaveIndex] = userGameSaveData;
        }
        string strs = JsonConvert.SerializeObject(userGameSaveDataList, JsonSerializerSettings);
        strs = EncryptDES(strs);
        string saveDataPath = $"{DataPath.gameSaveDataPath}{"/"}{userName}";
        File.WriteAllText(saveDataPath, strs);
    }

    public bool SetFishSaveData(int fish,int length,int place)
    {
        var saveData = UserGameSaveData;
        var fishDatas = saveData.fishSaveDatas;
        if (fishDatas == null)
        {
            fishDatas = new List<FishSaveData>();
        }
        bool newRecord = false;
        FishSaveData fishSaveData;
        int index = fishDatas.FindIndex(f => f.dataId == fish);
        if (index >= 0)
        {
            fishSaveData = fishDatas[index];
            if (fishSaveData.length < length)
            {
                fishSaveData.length = length;
                newRecord = true;
            }
            if (!fishSaveData.places.Contains(place))
            {
                fishSaveData.places.Add(place); 
            }
            fishDatas[index] = fishSaveData;
        }
        else
        {
            fishSaveData.length = length;
            fishSaveData.dataId = fish;
            fishSaveData.places = new List<int> { place };
            fishDatas.Add(fishSaveData);
            newRecord = true;
        }
        saveData.fishSaveDatas = fishDatas;
        userGameSaveDataList.userGameSaveDatas[selectSaveIndex] = saveData;
        return newRecord;
    }
    public FishSaveData GetFishDataSave(int id)
    {
        FishSaveData fishSaveData=default(FishSaveData);
        var fishDatas = UserGameSaveData.fishSaveDatas;
        if (fishDatas != null)
        {
            int index = fishDatas.FindIndex(f => f.dataId == id);
            if (index >= 0)
                fishSaveData = fishDatas[index];
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

    public bool SaveData(UserGameSaveData userGameSaveData)
    {
        selectSaveIndex = userGameSaveData.index;
        if (selectSaveIndex > 2)
        {
            return false;
        }
        //userGameSaveDataList.userGameSaveDatas[userGameSaveData.index] = userGameSaveData;
        SaveUserGameSaveData();
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