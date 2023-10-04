using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Data/多语言精灵资源")]
public class LanguageSpriteObj : ScriptableObject
{
    [System.Serializable]
    public struct LanguageSprite
    {
        public SystemLanguage systemLanguage;
        public Sprite sprite;
    }

    public Sprite defaultSprite;
    public List<LanguageSprite> LanguageSprites = new List<LanguageSprite>();

    private Dictionary<SystemLanguage, Sprite> LanguageSpriteDic = new Dictionary<SystemLanguage, Sprite>();

    void TryInitLanguageData()
    {
        if (LanguageSpriteDic.Count == 0)
        {
            for(int i=0;i<LanguageSprites.Count;i++)
            {
                LanguageSpriteDic[LanguageSprites[i].systemLanguage] = LanguageSprites[i].sprite;
            }
        }
    }
    public Sprite GetSprite()
    {
        TryInitLanguageData();
        SystemLanguage systemLanguage = Application.systemLanguage;
        if(LanguageSpriteDic.TryGetValue(systemLanguage,out Sprite sprite))
        {
            return sprite;
        }
        return defaultSprite; 
    }
    public Sprite GetSprite(SystemLanguage systemLanguage)
    {
        TryInitLanguageData(); 
        if (LanguageSpriteDic.TryGetValue(systemLanguage, out Sprite sprite))
        {
            return sprite;
        }
        return defaultSprite;
    }
}
