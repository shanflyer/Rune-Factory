using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
public class LanguageManage : Singleton<LanguageManage>
{  
    static List<SystemLanguage> Languages=new List<SystemLanguage> 
    {
        SystemLanguage.Chinese,
        SystemLanguage.Japanese,
        SystemLanguage.Korean,
        SystemLanguage.English
    }; 

    public static SystemLanguage nowLanguage;

    public override void Init()
    {
        base.Init(); 
    }
     
    public static void TextFanyi(Text text)
    {
        text.text = SwitchStr(text.text);
    }

    public void SystemLanguageMatch(bool SetLanguage, SystemLanguage SetSystemLanguage)
    {
        bool isMatch = false;
        if (!SetLanguage)
        { 
            nowLanguage = Application.systemLanguage;
            if (nowLanguage == SystemLanguage.ChineseSimplified || nowLanguage == SystemLanguage.ChineseTraditional ||
                nowLanguage == SystemLanguage.Chinese)
            {
                nowLanguage = SystemLanguage.Chinese;
            }
            foreach (var systemLanguage in Languages)
            {
                if (systemLanguage == nowLanguage)
                {
                    isMatch = true;
                    break;
                }
            }
            if (!isMatch)
            {
                nowLanguage=SystemLanguage.English;
            }
        }
        else
        {
            nowLanguage = SetSystemLanguage;
            foreach (var systemLanguage in Languages)
            {
                if (systemLanguage == nowLanguage)
                {
                    isMatch = true;
                    break;
                }
            }
            if (!isMatch)
            {
                nowLanguage = SystemLanguage.English;
            }
        }
        
    }
   

    public static string SwitchStr(string s)
    {
        var data = GameDataManager.instance.GetData<LangLanguageSwitch>(s);
        if (data.GetKey() == s)
        {
            return data.GetValue(nowLanguage);
        }
        return s;
    }  
    public string GameTimeToString(GameTime nowGameTime)
    {
        string result = "";
        if(nowLanguage == SystemLanguage.English)
        {

        }
        else
        {
            result = $"{nowGameTime.gameDate.year}年 {nowGameTime.gameDate.season} {nowGameTime.gameDate.date}日 【{nowGameTime.week}】";
        }

        return result;
    }
}
