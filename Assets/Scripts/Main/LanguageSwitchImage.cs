using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Image))]
public class LanguageSwitchImage :MonoBehaviour
{
    [SerializeField]
    List<LanguageImage> languageImages;
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
        SystemLanguage systemLanguage = LanguageManage.nowLanguage;
        SetImage(systemLanguage);
    }
    public void SetImage(SystemLanguage systemLanguage)
    {
        for(int i = 0; i < languageImages.Count; i++)
        {
            if (languageImages[i].systemLanguage == systemLanguage)
            {
                image.sprite = languageImages[i].sprite;
                if (isNativeSize)
                    image.SetNativeSize();
                break;
            }
        }
    }
}
[Serializable]
public struct LanguageImage
{
    public SystemLanguage systemLanguage;
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
