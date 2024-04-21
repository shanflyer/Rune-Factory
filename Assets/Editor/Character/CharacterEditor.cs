using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor; 
using UnityEngine;
using UnityEditor.Animations;
using System.Linq;

public class CharacterEditor : MyEditor
{
    [MenuItem("工具/角色动画")]
    public static void ShowWindowEditor()
    {
        CharacterEditor window = CreateWindow<CharacterEditor>("角色动画编辑");
        window.ShowAuxWindow();
    }
    private string sourcePath;
    private string copyAnimationPath;
    private string outAnimationPath;
    private string prefabPath;

    private GameObject copyPrefab;
    private void OnGUI()
    {
       DrawTextField(sourcePath,"资源路径",(string value) =>
       {
           sourcePath = value;
       },100,200);
        DrawTextField(copyAnimationPath, "复制动画路径", (string value) =>
        {
            copyAnimationPath = value;
        }, 100, 200);
        DrawTextField(outAnimationPath, "输出动画路径", (string value) =>
        {
            outAnimationPath = value;
        }, 100, 200);
        DrawTextField(prefabPath, "预制体路径", (string value) =>
        {
            prefabPath = value;
        }, 100, 200);
        copyPrefab=EditorGUILayout.ObjectField("复制的预制体", copyPrefab, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("复制动画"))
        {
            if (copyPrefab == null)
            {
                Debug.LogError("复制的预制体为空");
                return;
            }
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("资源路径为空");
                return;
            }
            if (string.IsNullOrEmpty(copyAnimationPath))
            {
                Debug.LogError("输出动画路径为空");
                return;
            }
            CopyAnimation();
        }
        if (GUILayout.Button("初始化动画"))
        {
            if (copyPrefab == null)
            {
                Debug.LogError("复制的预制体为空");
                return;
            }
            if (string.IsNullOrEmpty(sourcePath))
            {
                Debug.LogError("资源路径为空");
                return;
            }
            if (string.IsNullOrEmpty(copyAnimationPath))
            {
                Debug.LogError("输出动画路径为空");
                return;
            }
            InitAnimation();
        }
        if (GUILayout.Button("设置controller"))
        {

            SetOverrideAnimation();
        }
    }

    private void SetOverrideAnimation()
    {
        DirectoryInfo copyDir = new DirectoryInfo(copyAnimationPath);

        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{copyAnimationPath}/{copyDir.Name}.controller");
        var dirStrs = copyDir.Name.Split("/");
        oldModel = dirStrs[dirStrs.Length - 1];

        var files = copyDir.GetFiles("*.anim");
        Dictionary<string, AnimationClip> allAnimations = new Dictionary<string, AnimationClip>();
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fileName = file.Name;
            var animationName = fileName.Substring(0, fileName.Length - 5);
            var animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{copyAnimationPath}/{fileName}");
            if (animationClip == null)
            {
                Debug.LogError($"动画文件{fileName}不存在");
                continue;
            }
            allAnimations.Add(animationClip.name, animationClip);
        }

        Dictionary<string, Dictionary<string, Sprite>> characterSources = new Dictionary<string, Dictionary<string, Sprite>>();
        var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];
            if (asset is Sprite sprite)
            {
                var strs = sprite.name.Split('_');

                if (!characterSources.ContainsKey(strs[0]))
                {
                    characterSources[strs[0]] = new Dictionary<string, Sprite>();
                }
                characterSources[strs[0]][sprite.name] = sprite;
            }
        }

        try
        {
            AssetDatabase.StartAssetEditing();
             
            foreach (var data in characterSources)
            {
                if (data.Key == oldModel)
                {
                    continue;
                }

                string path = $"{outAnimationPath}/{data.Key}";
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var animaFiles = directoryInfo.GetFiles("*.anim");
                AnimatorOverrideController animatorOverrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>($"{path}/{data.Key}.overrideController");


                List<KeyValuePair<AnimationClip, AnimationClip>> animationClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                foreach (var animaFile in animaFiles)
                {
                    try
                    {
                        AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path + "/" + animaFile.Name);
                        if (allAnimations.TryGetValue(animationClip.name, out var animationClip1))
                        {
                            KeyValuePair<AnimationClip, AnimationClip> keyValuePair = new KeyValuePair<AnimationClip, AnimationClip>(animationClip1, animationClip);

                            animationClips.Add(keyValuePair);
                        }


                    }
                    catch
                    {
                        Debug.LogError(animaFile.FullName);
                    }

                }


                animatorOverrideController.ApplyOverrides(animationClips);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }



      
    }


    string oldModel = "";

    private void InitAnimation()
    {

        DirectoryInfo copyDir = new DirectoryInfo(copyAnimationPath);

        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{copyAnimationPath}/{copyDir.Name}.controller");
        var dirStrs = copyDir.Name.Split("/");
        oldModel = dirStrs[dirStrs.Length - 1];

        var files = copyDir.GetFiles("*.anim");

        Dictionary<string, AnimationClip> allAnimations = new Dictionary<string, AnimationClip>();
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fileName = file.Name;
            var animationName = fileName.Substring(0, fileName.Length - 5);
            var animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{copyAnimationPath}/{fileName}");
            if (animationClip == null)
            {
                Debug.LogError($"动画文件{fileName}不存在");
                continue;
            }
            allAnimations.Add(file.FullName, animationClip);
        }

        Dictionary<string, Dictionary<string, Sprite>> characterSources = new Dictionary<string, Dictionary<string, Sprite>>();
        var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
        for (int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];
            if (asset is Sprite sprite)
            {
                var strs = sprite.name.Split('_');

                if (!characterSources.ContainsKey(strs[0]))
                {
                    characterSources[strs[0]] = new Dictionary<string, Sprite>();
                }
                characterSources[strs[0]][sprite.name] = sprite;
            }
        }


        try
        {
            AssetDatabase.StartAssetEditing();



            foreach (var data in characterSources)
            {
                if (data.Key == oldModel)
                {
                    continue;
                }
                string path = $"{outAnimationPath}/{data.Key}";



                AnimatorOverrideController animatorOverrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>($"{path}/{data.Key}.overrideController");

                if (animatorOverrideController == null)
                {
                    animatorOverrideController = new AnimatorOverrideController();
                } 
                animatorOverrideController.name = data.Key;
                animatorOverrideController.runtimeAnimatorController = animatorController;
                if (!AssetDatabase.Contains(animatorOverrideController))
                {
                    AssetDatabase.CreateAsset(animatorOverrideController, $"{path}/{data.Key}.overrideController");
                }
                

               
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var animaFiles = directoryInfo.GetFiles("*.anim");

               
                foreach (var animaFile in animaFiles)
                {
                    AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path + "/" + animaFile.Name);
                   
                    var objBindings = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
                    foreach (var objBinding in objBindings)
                    {
                        var objkeyFrames = AnimationUtility.GetObjectReferenceCurve(animationClip, objBinding);
                        for (int i = 0; i < objkeyFrames.Length; i++)
                        { 
                            var keyFrame = objkeyFrames[i];
                            if (keyFrame.value == null)
                            {
                                continue;
                            }
                            var sprite = keyFrame.value as Sprite;
                            string sourceName = sprite.name.Replace(oldModel, data.Key);
                            if(data.Value.TryGetValue(sourceName, out sprite))
                            {
                                keyFrame.value = sprite;
                            }
                           
                            objkeyFrames[i] = keyFrame;
                        }
                        AnimationUtility.SetObjectReferenceCurve(animationClip, objBinding, objkeyFrames);
                    }

                    AssetDatabase.SaveAssets();
                }

                 

               

                GameObject obj = GameObject.Instantiate(copyPrefab);
                obj.name = data.Key;
                Animator animator = obj.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = animatorOverrideController;
                SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.sprite = data.Value.Values.ToList()[0];

                PrefabUtility.SaveAsPrefabAsset(obj, $"{prefabPath}/{data.Key}.prefab");
                GameObject.DestroyImmediate(obj);
                AssetDatabase.Refresh();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

    }
    void CopyAnimation()
    {
      

        DirectoryInfo copyDir = new DirectoryInfo(copyAnimationPath);

        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{copyAnimationPath}/{copyDir.Name}.controller");
        var dirStrs = copyDir.Name.Split("/");
        oldModel = dirStrs[dirStrs.Length - 1];

        var files = copyDir.GetFiles("*.anim"); 

        Dictionary<string, AnimationClip> allAnimations = new Dictionary<string, AnimationClip>();
        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fileName = file.Name;
            var animationName = fileName.Substring(0, fileName.Length - 5);
            var animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{copyAnimationPath}/{fileName}");
            if (animationClip == null)
            {
                Debug.LogError($"动画文件{fileName}不存在");
                continue;
            }
            allAnimations.Add(file.FullName, animationClip);             
        } 

        Dictionary<string, Dictionary<string, Sprite>> characterSources = new Dictionary<string, Dictionary<string, Sprite>>();
        var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
        for(int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];
            if(asset is Sprite sprite)
            {
                var strs = sprite.name.Split('_');
               
                if(!characterSources.ContainsKey(strs[0]))
                {
                    characterSources[strs[0]] = new Dictionary<string, Sprite>(); 
                }
                characterSources[strs[0]][sprite.name]= sprite; 
            }
        }

        try
        {
            AssetDatabase.StartAssetEditing(); 
            foreach (var data in characterSources)
            {
                if (data.Key == oldModel)
                {
                    continue;
                }
                string path = $"{outAnimationPath}/{data.Key}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (var copyAnimation in allAnimations)
                { 
                    string newPath = copyAnimation.Key.Replace(oldModel, data.Key);
                    if (newPath != copyAnimation.Key)
                    {
                        File.Copy(copyAnimation.Key, newPath, true);
                    } 
                } 
            }
            AssetDatabase.Refresh();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
       

       

    }


}
