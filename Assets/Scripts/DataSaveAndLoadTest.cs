using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;
using UnityEngine;
using System;
using System.Text;
using LitJson;
using System.IO;

public static class DataSaveAndLoadTest
{
    public static bool isJsonData;
    private static string Mu =$"{Application.persistentDataPath}/SaveData";

    private static string path0 = Application.persistentDataPath + @"/GameData.json";
    private static string path1 = Application.persistentDataPath + @"/playerData.json";
    private static string path2 = Application.persistentDataPath + @"/NpcData.json";
    private static string path3 = Application.persistentDataPath + @"/PlantData.json";
    private static string path4 = Application.persistentDataPath + @"/PastureData.json";
    private static string path5= Application.persistentDataPath + @"/deskData.json";
    private static string path6= Application.persistentDataPath + @"/playerMoneyData.json";
    private static string path7 = Application.persistentDataPath + @"/playerTitleData.json";
    private static string path8 = Application.persistentDataPath + @"/RedMoney.json";
    private static string path9 = Application.persistentDataPath + @"/SaveTime.json";

    private static string path11 = Application.persistentDataPath + @"/MarryTime.json";
    private static string path10 = Application.persistentDataPath + @"/PauseTime.json";
    private static string Mykey="i1jI0Ooz";
    public static GameSaveData gameSaveData;
    private static byte[] Keys = { 0x00, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

  


    public static void IniteZerodata()
    {
        gameSaveData=new GameSaveData();
        gameSaveData.ZeroInitData();
      
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

    public static void CreatPauseTimeData()
    {
        if (File.Exists(path10))
        {
            File.Delete(path10);
        }
        string DataStr = JsonMapper.ToJson(gameSaveData.pauseTime);
        string jsonStr = EncryptDES(DataStr, Mykey);
        FileStream fileStream = new FileStream(path10, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }

    public static void CreatDateData()
    {
        if (File.Exists(path0))
        {
            File.Delete(path0);
        }
        string DataStr = JsonMapper.ToJson(gameSaveData.dateData);
        string jsonStr = EncryptDES(DataStr, Mykey);
        FileStream fileStream = new FileStream(path0, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public static void CreatPlantData()
    {
        if (File.Exists(path3))
        {
            File.Delete(path3);
        }
        string DataStr3 = JsonMapper.ToJson(gameSaveData.plantSeveDatas);
        string jsonStr3 = EncryptDES(DataStr3, Mykey);
        FileStream fileStream3 = new FileStream(path3, FileMode.OpenOrCreate);
        StreamWriter sw3 = new StreamWriter(fileStream3);
        sw3.Write(jsonStr3);
        sw3.Close();
    }
    public static void CreatNPCData()
    {
        if (File.Exists(path2))
        {
            File.Delete(path2);
        }
        string DataStr2 = JsonMapper.ToJson(gameSaveData.npcSaveDatas);
        string jsonStr2 = EncryptDES(DataStr2, Mykey);
        FileStream fileStream2 = new FileStream(path2, FileMode.OpenOrCreate);
        StreamWriter sw2 = new StreamWriter(fileStream2);
        sw2.Write(jsonStr2);
        sw2.Close();
    }

    public static void CreatPlayerMoneyData()
    {
        if (File.Exists(path6))
        {
            File.Delete(path6);
        }
        string DataStr1 = JsonMapper.ToJson(gameSaveData.playerMoneyData);
        string jsonStr1 = EncryptDES(DataStr1, Mykey);
        FileStream fileStream1 = new FileStream(path6, FileMode.OpenOrCreate);
        StreamWriter sw1 = new StreamWriter(fileStream1);
        sw1.Write(jsonStr1);
        sw1.Close();
    }
    public static void CreatPlayerData()
    {
        if (File.Exists(path1))
        {
            File.Delete(path1);
        }
        string DataStr1 = JsonMapper.ToJson(gameSaveData.playerSaveData);
        string jsonStr1 = EncryptDES(DataStr1, Mykey);
        FileStream fileStream1 = new FileStream(path1, FileMode.OpenOrCreate);
        StreamWriter sw1 = new StreamWriter(fileStream1);
        sw1.Write(jsonStr1);
        sw1.Close();
    }

    public static void CreatPlayerTitleData()
    {
        if (File.Exists(path7))
        {
            File.Delete(path7);
        }
        string DataStr7 = JsonMapper.ToJson(gameSaveData.charactorTitleValue);
        string jsonStr7 = EncryptDES(DataStr7, Mykey);
        FileStream fileStream7 = new FileStream(path7, FileMode.OpenOrCreate);
        StreamWriter sw7 = new StreamWriter(fileStream7);
        sw7.Write(jsonStr7);
        sw7.Close();
    }
    public static void CreatPastureData()
    {
        if (File.Exists(path4))
        {
            File.Delete(path4);
        }
        string DataStr4 = JsonMapper.ToJson(gameSaveData.pastureSaveDatas);
        string jsonStr4 = EncryptDES(DataStr4, Mykey);
        FileStream fileStream4 = new FileStream(path4, FileMode.OpenOrCreate);
        StreamWriter sw4 = new StreamWriter(fileStream4);
        sw4.Write(jsonStr4);
        sw4.Close();
    }
    public static void CreatDeskData()
    {
        if (File.Exists(path5))
        {
            File.Delete(path5);
        }
        string DataStr5 = JsonMapper.ToJson(gameSaveData.deskData);
        string jsonStr5 = EncryptDES(DataStr5, Mykey);
        FileStream fileStream5 = new FileStream(path5, FileMode.OpenOrCreate);
        StreamWriter sw5 = new StreamWriter(fileStream5);
        sw5.Write(jsonStr5);
        sw5.Close();
    }
    public static void CreatNewData()
    {

        if (File.Exists(path7))
        {
            File.Delete(path7);
        }
        string DataStr7 = JsonMapper.ToJson(gameSaveData.charactorTitleValue);
        string jsonStr7 = EncryptDES(DataStr7, Mykey);
        FileStream fileStream7 = new FileStream(path7, FileMode.OpenOrCreate);
        StreamWriter sw7 = new StreamWriter(fileStream7);
        sw7.Write(jsonStr7);
        sw7.Close();

        if (File.Exists(path0))
        {
            File.Delete(path0);
        }
        string DataStr = JsonMapper.ToJson(gameSaveData.dateData);
        string jsonStr = EncryptDES(DataStr, Mykey);
        FileStream fileStream = new FileStream(path0, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();

        if (File.Exists(path1))
        {
            File.Delete(path1);
        }
        string DataStr1 = JsonMapper.ToJson(gameSaveData.playerSaveData);
        string jsonStr1 = EncryptDES(DataStr1, Mykey);

        //string dataStr1 = DecryptDES(jsonStr1, Mykey);

        FileStream fileStream1 = new FileStream(path1, FileMode.OpenOrCreate);
        StreamWriter sw1 = new StreamWriter(fileStream1);
        sw1.Write(jsonStr1);
        sw1.Close();

        if (File.Exists(path2))
        {
            File.Delete(path2);
        }
        string DataStr2 = JsonMapper.ToJson(gameSaveData.npcSaveDatas);
        string jsonStr2 = EncryptDES(DataStr2, Mykey);
        FileStream fileStream2 = new FileStream(path2, FileMode.OpenOrCreate);
        StreamWriter sw2 = new StreamWriter(fileStream2);
        sw2.Write(jsonStr2);
        sw2.Close();

        if (File.Exists(path3))
        {
            File.Delete(path3);
        }
        string DataStr3 = JsonMapper.ToJson(gameSaveData.plantSeveDatas);
        string jsonStr3 = EncryptDES(DataStr3, Mykey);
        FileStream fileStream3 = new FileStream(path3, FileMode.OpenOrCreate);
        StreamWriter sw3 = new StreamWriter(fileStream3);
        sw3.Write(jsonStr3);
        sw3.Close();

        if (File.Exists(path4))
        {
            File.Delete(path4);
        }
        string DataStr4 = JsonMapper.ToJson(gameSaveData.pastureSaveDatas);
        string jsonStr4 = EncryptDES(DataStr4, Mykey);
        FileStream fileStream4 = new FileStream(path4, FileMode.OpenOrCreate);
        StreamWriter sw4= new StreamWriter(fileStream4);
        sw4.Write(jsonStr4);
        sw4.Close();

        if (File.Exists(path5))
        {
            File.Delete(path5);
        }
        string DataStr5 = JsonMapper.ToJson(gameSaveData.deskData);
        string jsonStr5 = EncryptDES(DataStr5, Mykey);
        FileStream fileStream5 = new FileStream(path5, FileMode.OpenOrCreate);
        StreamWriter sw5 = new StreamWriter(fileStream5);
        sw5.Write(jsonStr5);
        sw5.Close();

        if (File.Exists(path6))
        {
            File.Delete(path6);
        }
        string DataStr6 = JsonMapper.ToJson(gameSaveData.playerMoneyData);
        string jsonStr6 = EncryptDES(DataStr6, Mykey);
        FileStream fileStream6 = new FileStream(path6, FileMode.OpenOrCreate);
        StreamWriter sw6 = new StreamWriter(fileStream6);
        sw6.Write(jsonStr6);
        sw6.Close();
    }
    public static void CreatMarryData()
    {
        if (File.Exists(path11))
        {
            File.Delete(path11);
        }
        string DataStr11 = JsonMapper.ToJson(gameSaveData.marryData);
        string jsonStr11 = EncryptDES(DataStr11, Mykey);
        FileStream fileStream11 = new FileStream(path11, FileMode.OpenOrCreate);
        StreamWriter sw11 = new StreamWriter(fileStream11);
        sw11.Write(jsonStr11);
        sw11.Close();
    }
    public static void CreatRedMoneyData()
    {
        if (File.Exists(path8))
        {
            File.Delete(path8);
        }
        string DataStr8 = JsonMapper.ToJson(gameSaveData.reMoney);
        string jsonStr8 = EncryptDES(DataStr8, Mykey);
        FileStream fileStream8 = new FileStream(path8, FileMode.OpenOrCreate);
        StreamWriter sw8 = new StreamWriter(fileStream8);
        sw8.Write(jsonStr8);
        sw8.Close();
    }

    public static bool LoadRedMoney()
    {
        if (File.Exists(path8))
        {
            gameSaveData.reMoney=new RedMoney();
            string jsonStr = File.ReadAllText(path8);
            string dataStr = DecryptDES(jsonStr, Mykey);
            gameSaveData.reMoney = JsonMapper.ToObject<RedMoney>(dataStr);
        }

        return false;
    }
    public static bool LoadOldUserData()
    {
        gameSaveData = new GameSaveData();
        if (!File.Exists(path0))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr = File.ReadAllText(path0);
        string dataStr = DecryptDES(jsonStr, Mykey);

        gameSaveData.dateData = JsonMapper.ToObject<DateData>(dataStr);

        if (!File.Exists(path7))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr7 = File.ReadAllText(path7);
        string dataStr7 = DecryptDES(jsonStr7, Mykey);
        gameSaveData.charactorTitleValue = JsonMapper.ToObject<CharactorTitleValue>(dataStr7);



        if (!File.Exists(path1))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr1 = File.ReadAllText(path1);
        string dataStr1 = DecryptDES(jsonStr1, Mykey);
        gameSaveData.playerSaveData = JsonMapper.ToObject<PlayerSaveData>(dataStr1);

        if (!File.Exists(path2))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr2 = File.ReadAllText(path2);
        string dataStr2 = DecryptDES(jsonStr2, Mykey);
        gameSaveData.npcSaveDatas = JsonMapper.ToObject<List<NpcSaveData>>(dataStr2);

        if (!File.Exists(path3))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr3 = File.ReadAllText(path3);
        string dataStr3 = DecryptDES(jsonStr3, Mykey);
        gameSaveData.plantSeveDatas = JsonMapper.ToObject<PlantFieldSeveData>(dataStr3);

        if (!File.Exists(path4))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr4 = File.ReadAllText(path4);
        string dataStr4 = DecryptDES(jsonStr4, Mykey);
        gameSaveData.pastureSaveDatas = JsonMapper.ToObject<List<PastureSaveData>>(dataStr4);

        if (!File.Exists(path5))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr5 = File.ReadAllText(path5);
        string dataStr5 = DecryptDES(jsonStr5, Mykey);
        gameSaveData.deskData = JsonMapper.ToObject<DeskData>(dataStr5);

        if (!File.Exists(path6))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr6 = File.ReadAllText(path6);
        string dataStr6 = DecryptDES(jsonStr6, Mykey);
        gameSaveData.playerMoneyData = JsonMapper.ToObject<PlayerMoneyData>(dataStr6);


        if (!File.Exists(path8))
        {
            gameSaveData.reMoney = new RedMoney();
            gameSaveData.reMoney.value = gameSaveData.playerMoneyData.money1;
            CreatRedMoneyData();
        }
        else
        {
            string jsonStr8 = File.ReadAllText(path8);
            string dataStr8 = DecryptDES(jsonStr8, Mykey);
            gameSaveData.reMoney = JsonMapper.ToObject<RedMoney>(dataStr8);
        }

        if (!File.Exists(path9))
        {
            gameSaveData.saveTime1 = new SaveTime();
            gameSaveData.saveTime1.value = gameSaveData.saveTime;
            CreatSaveTimedata();
        }
        else
        {
            string jsonStr9 = File.ReadAllText(path9);
            string dataStr9 = DecryptDES(jsonStr9, Mykey);
            gameSaveData.saveTime1 = JsonMapper.ToObject<SaveTime>(dataStr9);
        }
        if (!File.Exists(path10))
        {
            Debug.Log("没有数据！");
            gameSaveData.pauseTime = new PauseTime();
            //return false;
        }
        else
        {
            string jsonStr10 = File.ReadAllText(path10);
            string dataStr10 = DecryptDES(jsonStr10, Mykey);
            gameSaveData.pauseTime = JsonMapper.ToObject<PauseTime>(dataStr10);
        }
        if (!File.Exists(path11))
        {
            MarryData marryData = new MarryData();
            gameSaveData.marryData = marryData;
        }
        else
        {
            string jsonStr11 = File.ReadAllText(path11);
            string dataStr11 = DecryptDES(jsonStr11, Mykey);
            gameSaveData.marryData = JsonMapper.ToObject<MarryData>(dataStr11);
        }

        /*
         *   string _path = Application.dataPath + "/xxx.json";

                string _path1 = Application.dataPath + "/xxx1.json";
                 FileStream fileStream0 = new FileStream(_path, FileMode.OpenOrCreate);
                StreamWriter sw0 = new StreamWriter(fileStream0);
                sw0.Write(dataStr);
                sw0.Close();


         string DataStrx = File.ReadAllText(_path);
                  string jsonStrx = EncryptDES(DataStrx, Mykey);

                  FileStream fileStream = new FileStream(_path1, FileMode.OpenOrCreate);
                  StreamWriter sw = new StreamWriter(fileStream);
                  sw.Write(jsonStrx);
                  sw.Close();
                     */

        return true;
    }
    public static void CreatSaveTimedata()
    {
        if (File.Exists(path9))
        {
            File.Delete(path9);
        }
        string DataStr9 = JsonMapper.ToJson(gameSaveData.saveTime1);
        string jsonStr9 = EncryptDES(DataStr9, Mykey);
        FileStream fileStream9 = new FileStream(path9, FileMode.OpenOrCreate);
        StreamWriter sw9 = new StreamWriter(fileStream9);
        sw9.Write(jsonStr9);
        sw9.Close();
    }
    public static bool LoadUserData()
    {
        gameSaveData = new GameSaveData();
        if (File.Exists(Mu))
        {
            string jsonStr = File.ReadAllText(Mu);
            string dataStr = DecryptDES(jsonStr, Mykey);
            try
            {
                gameSaveData = JsonMapper.ToObject<GameSaveData>(dataStr);
            }
            catch (System.InvalidCastException e)
            { 
                return true;
            } 
        }
        else
        {
            LoadOldUserData();
        }

        return false;

    }

    public static bool LoadPauseTime()
    {
        if (!File.Exists(path10))
        {
            Debug.Log("没有数据！");
            gameSaveData.pauseTime = new PauseTime();
            return false;
        }
        string jsonStr10 = File.ReadAllText(path10);
        string dataStr10 = DecryptDES(jsonStr10, Mykey);
        gameSaveData.pauseTime = JsonMapper.ToObject<PauseTime>(dataStr10);
        return true;
    }
    public static bool CheckSaveData(int index) 
    {
        string savedataPath = Mu;
        if (index > -1)
        {
            savedataPath = $"{Mu}{index}";
        }
        return File.Exists(savedataPath);
    }
    public static void CreatSaveData(int index)
    {
        string savedataPath = Mu;
        if (index > -1)
        {
            savedataPath = $"{Mu}{index}";
        } 
        if (File.Exists(savedataPath))
        {
            File.Delete(savedataPath);
        }
        string DataStr = JsonMapper.ToJson(gameSaveData);
        string jsonStr = EncryptDES(DataStr, Mykey);
        FileStream fileStream = new FileStream(savedataPath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();  
    }

    public static void CopySaveData(int source, int target)
    {
        if (source == target)
        {
            return;
        }
        string sourceDataPath = Mu;
        if (source > -1)
        {
            sourceDataPath = $"{Mu}{source}";
        }

        string targetDataPath = Mu;
        if (target > -1)
        {
            targetDataPath = $"{Mu}{target}";
        } 

        string jsonStr = File.ReadAllText(sourceDataPath);

        FileStream fileStream = new FileStream(targetDataPath, FileMode.OpenOrCreate);
        StreamWriter sw = new StreamWriter(fileStream);
        sw.Write(jsonStr);
        sw.Close();
    }
    public static void DeletaSaveData(int index)
    {
        string savedataPath = Mu;
        if (index > -1)
        {
            savedataPath = $"{Mu}{index}";
        }
        if (File.Exists(savedataPath))
        {
            File.Delete(savedataPath);
        }
    }
    public static bool LoadSaveData(int index)
    {
        string savedataPath = Mu;
        if (index > -1)
        {
            savedataPath = $"{Mu}{index}";
        } 

        if (!File.Exists(savedataPath))
        {
            Debug.Log("没有数据！");
            return false;
        }
        string jsonStr = File.ReadAllText(savedataPath);
        string dataStr = DecryptDES(jsonStr, Mykey);

        GameSaveDataOld gameSaveDataOld = new GameSaveDataOld();
        try
        {
            gameSaveData = JsonMapper.ToObject<GameSaveData>(dataStr);
        }
        catch (System.InvalidCastException e)
        {

           
        }
        if (!File.Exists(path8))
        {
            gameSaveData.reMoney = new RedMoney();
            gameSaveData.reMoney.value = gameSaveData.playerMoneyData.money1;
        }
        if (!File.Exists(path9))
        {
            gameSaveData.saveTime1 = new SaveTime();
            gameSaveData.saveTime1.value = gameSaveData.saveTime;
        }
        if (!File.Exists(path10))
        {
            Debug.Log("没有数据ss！");
            gameSaveData.pauseTime = new PauseTime();

        }
        return true;
    }
}
