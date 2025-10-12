using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

public class OfflineSave:Singleton<OfflineSave>
{
    private MyOfflineSaveData myOfflineSaveData=new MyOfflineSaveData();
    public string UserName => userName;
    private string userName = "Waring";
    public override void Init()
    {
        base.Init(); 
    }
    public string saveFilePath=>Path.Combine(DataPath.gameSaveDataPath, userName); 
    public void SetUserName(string userName)
    {
       
        this.userName = SanitizeFileName(userName); 
    }
    string SanitizeFileName(string raw, int maxLen = 64)
    {
        if (string.IsNullOrEmpty(raw)) raw = "player";
        // 把空白换成下划线
        raw = Regex.Replace(raw, @"\s+", "_");
        // 只保留 A-Z a-z 0-9 _ -
        string s = Regex.Replace(raw, @"[^A-Za-z0-9_-]", "_");
        // 折叠多余下划线
        s = Regex.Replace(s, @"_+", "_");
        // 去两端的下划线/短横线
        s = Regex.Replace(s, @"^[_-]+|[_-]+$", "");
        if (string.IsNullOrEmpty(s)) s = "player";
        if (s.Length > maxLen) s = s.Substring(0, maxLen);
        return s;
    }
    public void RemoveKey(string key)
    {
        myOfflineSaveData.RemoveKey(key);
    }

    public void UpdateSaveFileData(string json,bool UpdateCloudData=true)
    {
        string path = saveFilePath;
         
        // 4) 原子写入：先写临时文件，再替换
        string tmp = path + ".tmp";
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        try
        {
            File.WriteAllText(tmp, json, utf8NoBom);

            // File.Replace 在部分平台可能不可用；做个兼容分支
            if (File.Exists(path))
            {
                try { File.Replace(tmp, path, null); }
                catch
                {
                    // 兼容方案（非严格原子）：先删后移
                    File.Delete(path);
                    File.Move(tmp, path);
                }
            }
            else
            {
                File.Move(tmp, path);
            }
            if(UpdateCloudData)
                GameSDKManager.instance.UpdateSaveData();
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e}");
            // 清理残留
            try { if (File.Exists(tmp)) File.Delete(tmp); } catch {}
            throw;
        }
    }
    public void SaveData(JsonSerializerSettings jsonSettings = null,bool UpdateCloudData=true)
    {
        string json = JsonConvert.SerializeObject(myOfflineSaveData);
        if (GameDataManager.instance.GlobalData.Encrypt)
        {
            json = EncryptDES(json);
        }

        UpdateSaveFileData(json,UpdateCloudData); 
    }
 
  
    public void LoadData()
    {
        string saveDataPath = saveFilePath;
        if (File.Exists(saveDataPath))
        { 
            string dataStr = File.ReadAllText(saveDataPath);
            UserGameSaveDataList userGameSaveDataList = null;
            try
            {
                myOfflineSaveData = JsonConvert.DeserializeObject<MyOfflineSaveData>(dataStr);
            }
            catch
            {
                dataStr = DecryptDES(dataStr);
                myOfflineSaveData  = JsonConvert.DeserializeObject<MyOfflineSaveData>(dataStr);
            } 
        }
        else
        {
            myOfflineSaveData=new MyOfflineSaveData();
        }
         
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
    
    
    public void SetInt(string key, int value)
    {
        myOfflineSaveData.SetInt(key, value);
    }

    public void SetString(string key, string value)
    { 
        myOfflineSaveData.SetString(key, value);
    }

    public int GetInt(string key)
    { 
        if (myOfflineSaveData == null)
        {
            return 0;
        }
        return myOfflineSaveData.GetInt(key); 
    }

    public string GetString(string key)
    {
        if (myOfflineSaveData == null)
        {
            return null;
        }
        return myOfflineSaveData.GetString(key);
    }

}