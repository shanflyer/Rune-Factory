using System;
using System.Collections.Generic;

public class MyOfflineSaveData
{
    public Dictionary<string,string> saveData=new Dictionary<string,string>();
    
    public void SetInt(string key, int value)
    {
        saveData[key] = value.ToString();
    }

    public void SetString(string key, string value)
    {
        saveData[key] = value;
    }

    public int GetInt(string key)
    {
        if (saveData.TryGetValue(key, out string value))
        {
            try
            {
                return int.Parse(value);
            }
            catch 
            {
                return -999;
            }
            
        }
        return 0;
    }

    public string GetString(string key)
    {
        if (saveData.TryGetValue(key, out string value))
        {
            return value;
        }

        return null;
    }

    public void RemoveKey(string key)
    {
        saveData.Remove(key);
    }
}