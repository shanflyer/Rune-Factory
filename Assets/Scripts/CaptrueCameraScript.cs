using UnityEngine;
using System.Collections;
using System;
using System.IO;
public class CaptrueCameraScript : MonoBehaviour
{
    public static string pic_fileName;
    //
    void Start()
    {
        int childNum = transform.childCount;
        for (int i = 0; i < childNum; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
            transform.GetChild(i).gameObject.SetActive(true);
            transform.GetChild(i).GetComponentInChildren<Animator>().SetBool("IsWalk",true);
        }
    }

    /// <summary>
    /// 对相机截屏,并保存
    /// </summary>
    /// <returns>The camera.</returns>
    /// <param name="camera">Camera.</param>
    /// <param name="rect">Rect.</param>
    public void CreatPng()
    {
        CaptrueCamera(GetComponent<Camera>(), new Rect(Vector2.zero, new Vector2(1136.0f, 640.0f)));
    }
    public Texture2D CaptrueCamera(Camera camera, Rect rect)
    {
        // 创建一个RenderTexture对象  
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
        camera.targetTexture = rt;
        camera.Render();
        // 激活这个rt, 并从中中读取像素。  
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示  
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;  
        RenderTexture.active = null; // JC: added to avoid errors  
        GameObject.Destroy(rt);
        string formatType = ".png";
        string formatTime = DateTime.Now.ToString();
        formatTime = formatTime.Replace('/', '-').Replace(' ', '-').Replace(':', '-').ToString();
        // 最后将这些纹理数据，成一个png图片文件  
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = null;
        filename = Application.dataPath + "/" + formatTime + formatType;
        /*
#if UNITY_ANDROID
        filename = Application.persistentDataPath + "/" + formatTime + formatType;
        Debug.Log("android!");
#elif UNITY_IPHONE
                    filename = Application.persistentDataPath + "/" + formatTime + formatType;
        Debug.Log("ios");
#elif UNITY_EDITOR
           filename = Application.dataPath + "/" + formatTime + formatType;
        Debug.Log("editor");
#endif
*/
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("截屏了一张照片: {0}", filename));

        return screenShot;
    }
    private IEnumerator ReadFileLoadTex()
    {
        yield return 0;
        string path = null;
#if UNITY_ANDROID
        path = Application.persistentDataPath + "/" + pic_fileName;
#elif UNITY_IPHONE
                    path=Application.persistentDataPath+"/"+pic_fileName;
#elif UNITY_EDITOR
                    path=Application.dataPath+"/"+pic_fileName;
#endif
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        fs.Seek(0, SeekOrigin.Begin);
        //创建文件长度缓存
        byte[] bytes = new byte[fs.Length];
        //读取文件
        fs.Read(bytes, 0, (int)fs.Length);
        //释放文件读取流
        fs.Close();
        fs.Dispose();
        fs = null;

        //创建texture
        Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.LoadImage(bytes);
        //      图片赋值
        //              GameObject.Find ("UI Root/Texture").GetComponent<UITexture> ().mainTexture = tex;
        //              GameObject.Find ("Canvas/RawImage").GetComponent<RawImage> ().texture = tex;
    }

}