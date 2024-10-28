using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
public class IOSTest : MonoBehaviour
{
#if UNITY_EDITOR
    private string url = "itms-apps://itunes.apple.com/cn/app/id1449447537";
    private string imagePath;
    private string imageDataString;

    void Start()
    {
        
        imagePath = Application.persistentDataPath + "/001.png";
    }

    public void SharWebUrl()
    {
        string notice = LanguageManage.SwitchStr("我发现的一款好玩的游戏");
        if (GameTimeManager.instance.Season ==Season.春&&GameTimeManager.instance.Year!=1300)
        {
            notice = LanguageManage.SwitchStr("我已经在这个游戏中度过") +
                (GameTimeManager.instance.Year - 1300 )+ LanguageManage.SwitchStr("年了！");
        }
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            ShareWeb(notice, url, imagePath, "subject");
        }
        else
        {
           // GameComponentData.gameData.gameManager.ShareSuccess();
        }

       
    }
    /*
    void OnGUI()
    {
        if (GUILayout.Button("SocialSharing", GUILayout.Width(200), GUILayout.Height(100)))
        {
            // ShareIOS("测试", "test");
            SocialSharing("测试"+Environment.NewLine+"wwwww"+Environment.NewLine+"sssssssssss",url,imageDataString,"subject");
        }
        if (GUILayout.Button("ShareWeb", GUILayout.Width(200), GUILayout.Height(100)))
        {
            ShareWeb("测试"+Environment.NewLine+""+Environment.NewLine+"sssssssssss",url,imageDataString,"subject");
        }
        if (GUILayout.Button("Share image", GUILayout.Width(200), GUILayout.Height(100)))
        {
            // 分享图片
            SocialSharing("","",imageDataString,"subject");
        }
         if (GUILayout.Button("Share image path", GUILayout.Width(200), GUILayout.Height(100)))
        {
            // 分享图片
            SocialSharing("","",imagePath,"subject");
        }
        if (GUILayout.Button("Screen Shot", GUILayout.Width(200), GUILayout.Height(100)))
        {
           StartCoroutine(ShareScreenShot());
        }
    }
    */

    IEnumerator ShareScreenShot()
    {
        yield return new WaitForSeconds(1f);
        ScreenCapture.CaptureScreenshot("ScreensShot.png");
        yield return new WaitForSeconds(1f);
        // NativeShare.Share("测试", Application.persistentDataPath + "/ScreenShot.png", "https://www.baidu.com");
        // showSocialSharing("测试",Application.persistentDataPath+"/ScreenShot.png");
        // (char* body, char* url, char* imageDataString, char* subject)
        using (WWW www = new WWW(imagePath))
        {
            yield return www;
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(www.bytes);
            imageDataString = Convert.ToBase64String(texture.EncodeToPNG());
        }


    }
  
    [DllImport("__Internal")]
    private static extern void SocialSharing(string body, string url, string imageDataString, string subject);
    [DllImport("__Internal")]
    private static extern void ShareWeb(string body, string url, string imagePath, string subject);
#endif
}

