using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor; 
using UnityEngine;
using UnityEditor.Animations;

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
       });
        DrawTextField(copyAnimationPath, "复制动画路径", (string value) =>
        {
            copyAnimationPath = value;
        });
        DrawTextField(outAnimationPath, "输出动画路径", (string value) =>
        {
            outAnimationPath = value;
        });
        DrawTextField(prefabPath, "预制体路径", (string value) =>
        {
            prefabPath = value;
        });
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
    }
    void CopyAnimation()
    {
      

        DirectoryInfo copyDir = new DirectoryInfo(copyAnimationPath);

        AnimatorController animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{copyAnimationPath}/{copyDir.Name}.controller");


        var files = copyDir.GetFiles("*.anim");
        Dictionary<string, AnimationClip> IdleAnimations = new Dictionary<string, AnimationClip>();
        Dictionary<string, AnimationClip> WalkAnimations = new Dictionary<string, AnimationClip>();

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
            var actionName = animationName.Split('_')[0];
            var directName = animationName.Split('_')[1];
            if (animationClip.name.Contains("Idle"))
            {
                IdleAnimations[directName] = animationClip;
            }
            else
            {
                WalkAnimations[directName] = animationClip;
            }
        }


        Dictionary<string, Dictionary<string, List<Sprite>>> characterSources = new Dictionary<string, Dictionary<string, List<Sprite>>>();
        var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
        for(int i = 0; i < assets.Length; i++)
        {
            var asset = assets[i];
            if(asset is Sprite sprite)
            {
                var strs = sprite.name.Split('_');
                if (strs.Length < 3)
                {
                    Debug.LogError($"资源{sprite.name}命名不规范");
                    continue;
                }
                 if(!characterSources.ContainsKey(strs[0]))
                {
                    characterSources[strs[0]] = new Dictionary<string, List<Sprite>>(); 
                }
                 if (!characterSources[strs[0]].ContainsKey(strs[1]))
                {
                    characterSources[strs[0]][strs[1]] = new List<Sprite>();
                }
                characterSources[strs[0]][strs[1]].Add(sprite);
            }
        }

        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var data in characterSources)
            {
                string path = $"{outAnimationPath}/{data.Key}";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (var copyAnimation in allAnimations)
                {

                    AnimationClip animationClip = new AnimationClip();
                    animationClip.name = copyAnimation.Key;


                    var objBlind = AnimationUtility.GetObjectReferenceCurveBindings(copyAnimation.Value);
                    foreach (var blind in objBlind)
                    {
                        var objCurves = AnimationUtility.GetObjectReferenceCurve(copyAnimation.Value, blind);
                        AnimationUtility.SetObjectReferenceCurve(animationClip, blind, objCurves);
                    }
                   

                    var curveBlind = AnimationUtility.GetCurveBindings(copyAnimation.Value);
                    foreach(var curveblind in curveBlind)
                    {
                        var curve = AnimationUtility.GetEditorCurve(copyAnimation.Value, curveblind);
                        AnimationUtility.SetEditorCurve(animationClip, curveblind, curve);
                    }
                    AnimationUtility.SetAnimationClipSettings(animationClip, AnimationUtility.GetAnimationClipSettings(copyAnimation.Value));


                    AssetDatabase.CreateAsset(animationClip, $"{path}/{copyAnimation.Value.name}.anim");
                } 
            }
            AssetDatabase.Refresh();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        try
        {
            AssetDatabase.StartAssetEditing();

          

            foreach (var data in characterSources)
            { 
                AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController();
                animatorOverrideController.name = data.Key;
                animatorOverrideController.runtimeAnimatorController = animatorController;

                string path = $"{outAnimationPath}/{data.Key}";
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var animaFiles = directoryInfo.GetFiles("*.anim");

                List<KeyValuePair<AnimationClip, AnimationClip>> animationClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                foreach (var animaFile in animaFiles)
                {
                    AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path + "/" + animaFile.Name);
                    if(allAnimations.TryGetValue(animationClip.name,out var animationClip1))
                    {
                        KeyValuePair<AnimationClip, AnimationClip> keyValuePair = new KeyValuePair<AnimationClip, AnimationClip>(animationClip1, animationClip);
                       
                        animationClips.Add(keyValuePair); 
                    }

                    animationClip.frameRate = animationClip1.frameRate;

                    var strs = animaFile.Name.Split('.')[0].Split('_');
                    string directName = strs[1];
                    if (directName == "左")
                    {
                        directName = "右";
                    }
                    if (data.Value.TryGetValue(directName, out var sprites))
                    {
                        var objBlind = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
                        foreach (var blind in objBlind)
                        {
                            var objCurves = AnimationUtility.GetObjectReferenceCurve(animationClip, blind);
                            if (strs[0].Contains("Idle"))
                            {
                                for (int i = 0; i < objCurves.Length; i++)
                                {
                                    objCurves[i].value = sprites[1];
                                }
                            }
                            else
                            {
                                for (int i = 0; i < objCurves.Length; i++)
                                {
                                    objCurves[i].value = i < sprites.Count ? sprites[i] : sprites[1];
                                }
                            }
                            AnimationUtility.SetObjectReferenceCurve(animationClip, blind, objCurves);
                        }
                    }
                    AssetDatabase.SaveAssets();
                }


                animatorOverrideController.ApplyOverrides(animationClips);

                AssetDatabase.CreateAsset(animatorOverrideController, $"{path}/{data.Key}.overrideController");

                GameObject obj = GameObject.Instantiate(copyPrefab);
                obj.name = data.Key;
                Animator animator = obj.GetComponentInChildren<Animator>();
                animator.runtimeAnimatorController = animatorOverrideController;
                SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
                spriteRenderer.sprite= data.Value["右"][1];

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


}
