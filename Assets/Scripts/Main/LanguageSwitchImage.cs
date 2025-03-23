using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Unity.Entities.UniversalDelegates;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Image))]
public class LanguageSwitchImage :MonoBehaviour
{
    [SerializeField]
    LanguageSpriteObj LanguageSpriteObj;
    [SerializeField]
    Image image;
    [SerializeField]
    bool isNativeSize;

   
    private void OnEnable()
    {
        if (image == null)
        {
            image = gameObject.GetComponent<Image>();
        }
        MyLanguage systemLanguage = LanguageManage.nowLanguage;
        SetImage(systemLanguage);
    }
    public void SetImage(MyLanguage MyLanguage)
    {
        image.sprite = LanguageSpriteObj.GetSprite(MyLanguage);
        if (isNativeSize)
            image.SetNativeSize();
    }
}
[Serializable]
public struct LanguageImage
{
    public MyLanguage systemLanguage;
    public Sprite sprite;
}
#if UNITY_EDITOR
[CustomEditor(typeof(LanguageSwitchImage))]
public class LanguageSwitchImageEditor : Editor
{
    public LanguageSwitchImage languageSwitchImage => target as LanguageSwitchImage;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("InitImage"))
        {
            languageSwitchImage.SetImage(LanguageManage.nowLanguage);
        }
    }
}
#endif
