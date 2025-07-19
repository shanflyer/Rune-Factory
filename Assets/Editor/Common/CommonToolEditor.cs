using Mono.Cecil.Cil;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.PSD;
using UnityEngine;
using UnityEngine.U2D;

public class CommonToolEditor : MyEditor
{
    public static CommonToolEditor Instance;

    [MenuItem("工具/通用工具")]
    public static void WindowShow()
    {
        Instance = EditorWindow.CreateWindow<CommonToolEditor>("通用工具");
        Instance.minSize = new Vector2(240, 360);
        Instance.maxSize = new Vector2(240, 360);
        Instance.ShowAuxWindow();
    }

    private void OnGUI()
    {
       
        sourcePath = EditorGUILayout.TextField("源路径", sourcePath);
        animationPath = EditorGUILayout.TextField("输出路径", animationPath);
        if (GUILayout.Button("生成表情动画"))
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                var assets = AssetDatabase.LoadAllAssetsAtPath(sourcePath);

                int index = 0;
                List<List<Sprite>> clipSprites = new List<List<Sprite>>();
                for (int i = 0; i < assets.Length; i++)
                {
                    if (assets[i] is Sprite sprite)
                    {
                        int d = index % 3;
                        if (d == 0)
                        {
                            List<Sprite> sprites = new List<Sprite>();
                            clipSprites.Add(sprites);
                        }
                        clipSprites[clipSprites.Count - 1].Add(sprite);
                        index++;
                    }
                }
                for (int i = 0; i < clipSprites.Count; i++)
                {
                    var sprites = clipSprites[i];
                    AnimationClip animationClip = new AnimationClip();
                    animationClip.frameRate = 4;
                    animationClip.legacy = true;
                    animationClip.name = i.ToString();
                    ObjectReferenceKeyframe[] objectReferenceKeyframes = new ObjectReferenceKeyframe[3]
                    {
                    new ObjectReferenceKeyframe
                    {
                        time=0,
                        value=sprites[0]
                    },
                    new ObjectReferenceKeyframe
                    {
                        time=0.25f,
                        value=sprites[1]
                    },
                    new ObjectReferenceKeyframe
                    {
                        time=0.5f,
                        value=sprites[2]
                    }
                    };
                    EditorCurveBinding binding = new EditorCurveBinding
                    {
                        type = typeof(SpriteRenderer),
                        propertyName = "m_Sprite"
                    };
                    AnimationUtility.SetObjectReferenceCurve(animationClip, binding, objectReferenceKeyframes);

                    AnimationClipSettings animationClipSettings = new AnimationClipSettings
                    {
                        startTime = 0,
                        stopTime = 0.75f
                    };
                    AnimationUtility.SetAnimationClipSettings(animationClip, animationClipSettings);
                    AssetDatabase.CreateAsset(animationClip, $"{animationPath}{animationClip.name}.anim");
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        if (GUILayout.Button("生成植物预制体"))
        {
            CreatPlantMapItemObj();
        }
        if (GUILayout.Button("生成植物数据"))
        {
            CreatPlantMapItemData();
        }
        if (GUILayout.Button("初始话植物动画"))
        {
            InitPlantAnimation();
        }
        if (GUILayout.Button("标准化名字"))
        {
            ReSaveImage();
        }
        if (GUILayout.Button("标准资源"))
        {
            AddReSaveImage();
        }
        if (GUILayout.Button("重新保存灯光变化"))
        {
            ReSavePrefab();
        }

        oldSourcePath=EditorGUILayout.TextField("dataPath",oldSourcePath);
        newSourecePath = EditorGUILayout.TextField("newSourcePath", newSourecePath);
       // if (GUILayout.Button("Test serial"))
        {
           // ResetInstance();
        }
        if (GUILayout.Button("室外映射"))
        {
            SetSprite();
        }

        if (GUILayout.Button("刷新TestRender"))
        {
            UpdateObjTestRender();
        }
        if (GUILayout.Button("输出UI内容"))
        {
            OutUIText();
        }
        objPath = EditorGUILayout.TextField("物体路径", objPath);
        staticObj = EditorGUILayout.Toggle("静态物体", staticObj);
        if (GUILayout.Button("改变物体缩放组织形式"))
        {
            ChangeObjScale(objPath); 
        }

        if (GUILayout.Button("替换SpriteRenderer"))
        {
            ReplaceSpriteRenderer();
        }
        if (GUILayout.Button("替换Sprite动画"))
        {
            ReplaceSpriteAnimation();
        }
        if (GUILayout.Button("增加角色 scale 动画"))
        {
            AddSpriteScaleAnimation();
        }
        if (GUILayout.Button("移除miss"))
        {
            RemoveMissComp();
        }
         
        originalPsbPath= EditorGUILayout.TextField("源Psd", originalPsbPath);
        targePsbPath = EditorGUILayout.TextField("目标路径", targePsbPath);
        if (GUILayout.Button("复制"))
        {
            CopyPsdImporter();
        }
        /*
        if (GUILayout.Button("USE_SHAPE_LIGHT_TYPE_0"))
        {
            Shader.DisableKeyword("USE_SHAPE_LIGHT_TYPE_0");
        }
        if (GUILayout.Button("USE_SHAPE_LIGHT_TYPE_1"))
        {
            Shader.DisableKeyword("USE_SHAPE_LIGHT_TYPE_1");
        }
        if (GUILayout.Button("USE_SHAPE_LIGHT_TYPE_2"))
        {
            Shader.DisableKeyword("USE_SHAPE_LIGHT_TYPE_2");
        }
        if (GUILayout.Button("USE_SHAPE_LIGHT_TYPE_3"))
        {
            Shader.DisableKeyword("USE_SHAPE_LIGHT_TYPE_3");
        }*/

    }
    string objPath = "";
    bool staticObj = true;
    public void AddSpriteScaleAnimation()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(objPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            var files = directoryInfo.GetFiles("*.anim");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var prefabPath = objPath + file.Name;

                AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(prefabPath);
                SetAnimation(animationClip);

            }
            var dirs = directoryInfo.GetDirectories();
            foreach (var d in dirs)
            {
                var files1 = d.GetFiles("*.anim");
                for (int i = 0; i < files1.Length; i++)
                {
                    var file = files1[i];
                    var prefabPath = objPath + d.Name + "/" + file.Name;

                    AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(prefabPath);
                    SetAnimation(animationClip);

                }
            }


            void SetAnimation(AnimationClip animationClip)
            {
                if (animationClip.name.Contains("_Left"))
                {
                    return;
                }
                if (animationClip.name.Contains("_Right"))
                {
                    return;
                }

                EditorCurveBinding editorCurveBinding = new EditorCurveBinding
                {
                    type = typeof(Transform),
                    path = "Body",
                    propertyName = "m_LocalScale.x",
                };
                Keyframe[] keyframes = new Keyframe[]
                {
                    new Keyframe
                    {
                        time=0,
                        value=1
                    }
                };
                AnimationCurve animationCurve = new AnimationCurve
                {
                    keys = keyframes
                }; AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, animationCurve);

                
                AssetDatabase.SaveAssets();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
    public void ReplaceSpriteAnimation()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(objPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            var files = directoryInfo.GetFiles("*.anim");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var prefabPath = objPath + file.Name;

                AnimationClip animationClip= AssetDatabase.LoadAssetAtPath<AnimationClip>(prefabPath);
                SetAnimation(animationClip);

            }
            var dirs = directoryInfo.GetDirectories();
            foreach(var d in dirs)
            {
                var files1=d.GetFiles("*.anim");
                for (int i = 0; i < files1.Length; i++)
                {
                    var file = files1[i];
                    var prefabPath = objPath+d.Name+"/" + file.Name;

                    AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(prefabPath);
                    SetAnimation(animationClip);

                }
            }


            void SetAnimation(AnimationClip animationClip)
            {
                var Bindings = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
                for (int j = 0; j < Bindings.Length; j++)
                {
                    var binding = Bindings[j];
                    if (binding.type == typeof(SpriteRenderer))
                    {
                        var keys = AnimationUtility.GetObjectReferenceCurve(animationClip, binding);



                        EditorCurveBinding editorCurveBinding = new EditorCurveBinding
                        {
                            type = typeof(MySpriteMeshRender),
                            path = binding.path,
                            propertyName = binding.propertyName,
                        };
                        AnimationUtility.SetObjectReferenceCurve(animationClip, editorCurveBinding, keys);
                        AnimationUtility.SetObjectReferenceCurve(animationClip, Bindings[j], null);
                    }

                    //var keyObjs= AnimationUtility.GetObjectReferenceCurve(animationClip, binding);
                }
                var curveBindings = AnimationUtility.GetCurveBindings(animationClip);
                for (int j = 0; j < curveBindings.Length; j++)
                {
                    var binding = curveBindings[j];
                    if (binding.type == typeof(SpriteRenderer))
                    {
                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);

                        if (binding.propertyName == "m_FlipX")
                        {
                            EditorCurveBinding editorCurveBinding = new EditorCurveBinding
                            {
                                type = typeof(Transform),
                                path = binding.path,
                                propertyName = "m_LocalScale.x"
                            };
                            var keys = curve.keys;
                            for(int  k = 0; k < keys.Length; k++)
                            {
                                if (keys[k].value == 0)
                                {
                                    keys[k].value = 1;
                                }
                                else
                                if (keys[k].value == 1)
                                {
                                    keys[k].value = -1;
                                }
                            }
                            curve.keys = keys;
                            AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, curve);
                            AnimationUtility.SetEditorCurve(animationClip, binding, null);
                        }
                        else
                        {
                            EditorCurveBinding editorCurveBinding = new EditorCurveBinding
                            {
                                type = typeof(MySpriteMeshRender),
                                path = binding.path,
                                propertyName = binding.propertyName
                            };

                            AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, curve);
                            AnimationUtility.SetEditorCurve(animationClip, binding, null);
                        }

                    }
                }
                AssetDatabase.SaveAssets();
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    string originalPsbPath;
    string targePsbPath;
    public void CopyPsdImporter()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            DirectoryInfo directoryInfo = new DirectoryInfo(targePsbPath);
            var files = directoryInfo.GetFiles("*.psb");
            foreach (var file in files)
            {
                var filePath = $"{targePsbPath}/{file.Name}";
                CopyBone(originalPsbPath, filePath);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
       
    }
    void CopyBone(string originalPath,string targetPath)
    {
        if (originalPath == targetPath)
        {
            return;
        }
        PSDImporter original = (AssetImporter.GetAtPath(originalPath) as PSDImporter);
        PSDImporter target = (AssetImporter.GetAtPath(targetPath) as PSDImporter);
        var data2 = target.m_CharacterData;
        var data1= original.m_CharacterData;
        data2.bones = data1.bones;

        List<SpriteBone> spriteBones = original.m_CharacterData.bones.ToList();

        target.m_CharacterData.bones=spriteBones.ToArray();
        target.SaveAndReimport();

    }

    public void RemoveMissComp()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(objPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            var files = directoryInfo.GetFiles("*.Prefab");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var prefabPath = objPath + file.Name;
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
               
               
                
             GameObject _obj = (GameObject)PrefabUtility.InstantiatePrefab(gameObject);
                var children = _obj.GetComponentsInChildren<Transform>();
                foreach (var child in children)
                {
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(child.gameObject);
                }
                PrefabUtility.SaveAsPrefabAssetAndConnect(_obj, prefabPath, InteractionMode.AutomatedAction);
             DestroyImmediate(_obj);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }
    public void ReplaceSpriteRenderer()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(objPath);
        try
        {
            AssetDatabase.StartAssetEditing();
            var files = directoryInfo.GetFiles("*.Prefab");
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var prefabPath = objPath + file.Name;
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                HashSet<SpriteRenderer> lightSpriteRenders = new HashSet<SpriteRenderer>();
                GameObject _obj = (GameObject)PrefabUtility.InstantiatePrefab(gameObject);
                var myLights = _obj.GetComponentsInChildren<MyLight>(true);
                foreach(var myLight in myLights)
                {
                    foreach(var r in myLight.spriteRenderers)
                    {
                        lightSpriteRenders.Add(r);
                    }
                }
                var spriteRenders = _obj.GetComponentsInChildren<SpriteRenderer>(true);
                for (int j = 0; j < spriteRenders.Length; j++)
                {
                    if (lightSpriteRenders.Contains(spriteRenders[j]))
                    {
                        continue;
                    }
                    if (spriteRenders[j].TryGetComponent(out MyLightSprite myLightSprite))
                    {
                        continue;
                    }
                    if (spriteRenders[j].drawMode != SpriteDrawMode.Simple)
                    {
                        continue;
                    }
                    GameObject s_obj = spriteRenders[j].gameObject;
                    Sprite sprite = spriteRenders[j].sprite;
                    Material material = spriteRenders[j].sharedMaterial;
                    Color color = spriteRenders[j].color;
                    DestroyImmediate(spriteRenders[j]);
                    MySpriteMeshRender mySpriteMeshRender = s_obj.AddComponent<MySpriteMeshRender>();
                    mySpriteMeshRender.m_Sprite = sprite;
                    mySpriteMeshRender.m_Material = material;
                    mySpriteMeshRender.m_Color = color;
                }
                PrefabUtility.SaveAsPrefabAssetAndConnect(_obj, prefabPath, InteractionMode.AutomatedAction);
                DestroyImmediate(_obj);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
       
    }
   
    void ChangeObjScale(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        try
        {
            AssetDatabase.StartAssetEditing();
            InitObjScale(directoryInfo, path);
            void InitObjScale(DirectoryInfo directoryInfo, string path)
            {
                var files = directoryInfo.GetFiles("*.Prefab");
                for (int i = 0; i < files.Length; i++)
                {
                    string objPath = $"{path}/{files[i].Name}";
                    GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(objPath);
                    GameObject _obj =(GameObject)PrefabUtility.InstantiatePrefab(obj);
                   
                    var spriteMeshRenders = _obj.GetComponentsInChildren<MySpriteMeshRender>(true);
                    foreach (var s in spriteMeshRenders)
                    {
                        s.flip = s.transform.localScale.x < 0;
                        if (math.abs(s.transform.localScale.x) != 1|| math.abs(s.transform.localScale.y) != 1)
                        {
                            s.Size = new Vector2(s.transform.localScale.x, s.transform.localScale.y);
                        }
                         else if (s.size != 1)
                        {
                          s.Size = new Vector2(s.size, math.abs(s.size));
                            
                        }
                        if (math.abs(s.transform.localScale.x) != math.abs(s.transform.localScale.y))
                        {
                            Debug.Log($"{_obj.name}-{s.transform.name}");
                        }
                    }
                    PrefabUtility.SaveAsPrefabAsset(_obj, objPath);
                    GameObject.DestroyImmediate(_obj);
                    AssetDatabase.Refresh();
                }
                var dirs = directoryInfo.GetDirectories();
                foreach (var dir in dirs)
                {
                    InitObjScale(dir, $"{path}/{dir.Name}");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
       
    }
    void AddObjPosData(string path)
    {
        LayerMask lightLayer = LayerMask.NameToLayer("Light");
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
       
      
    }

    private void OutUIText()
    {
        string uiPath = "Prefabs/UI";
        var objs = Resources.LoadAll<GameObject>(uiPath);
        string outStr = " ";
        foreach ( var obj in objs )
        {
            var texts = obj.GetComponentsInChildren<TMP_Text>();
            foreach(var t in texts)
            {
                outStr = $"{outStr}\n{t.text}";
            }
        }
        File.WriteAllText("OutText", outStr);
    }

    private void UpdateObjTestRender()
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            string MapPath = "Assets/Resources/Prefabs/Ground";
            string ObjPath = "Assets/Resources/Prefabs/MapItem";
            DirectoryInfo directoryInfo = new DirectoryInfo(MapPath);
            var files = directoryInfo.GetFiles("*.Prefab");
           
            DirectoryInfo directoryInfo1 = new DirectoryInfo(ObjPath);
            var files1 = directoryInfo1.GetFiles("*.Prefab");
            foreach (var file in files1)
            {
                GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>($"{ObjPath}/{file.Name}");
                
                EditorUtility.SetDirty(obj);
            }
            AssetDatabase.Refresh();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
       
    }


    string oldSourcePath = "";
    string newSourecePath = "";

 

    public void SetSprite()
    {
        Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        var sources = AssetDatabase.LoadAllAssetsAtPath(newSourecePath);
        foreach(var source in sources)
        {
            if(source is Sprite sprite)
            {
                sprites.Add(sprite.name, sprite);
            }
        }


        DirectoryInfo dir = new DirectoryInfo(oldSourcePath);
        var files = dir.GetFiles("*.prefab");
        try
        {
            AssetDatabase.StartAssetEditing();
            foreach (var file in files)
            {
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(oldSourcePath + file.Name);
                var spriteRenderers = obj.GetComponentsInChildren<SpriteRenderer>();
               bool reSave = false;
                foreach (var spriteRenderer in spriteRenderers)
                {
                    if (spriteRenderer.sprite != null && sprites.TryGetValue(spriteRenderer.sprite.name, out var sprite))
                    {
                        spriteRenderer.sprite = sprite;
                        reSave = true;
                    }
                }
                if(reSave)
                    AssetDatabase.SaveAssetIfDirty(obj);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
      
    }

    public class ReMapSpriteData
    {
        public Dictionary<string, string> Remap;
    }

    public void ResetInstance()
    {
        if(File.Exists(oldSourcePath))
        {
             var strs = File.ReadAllText(newSourecePath);

            var datas = File.ReadAllText(oldSourcePath);
            var remapData = JsonConvert.DeserializeObject<ReMapSpriteData>(datas);
            foreach(var d in remapData.Remap)
            {
                strs=strs.Replace(d.Key,d.Value);
            }
            File.WriteAllText(newSourecePath, strs);
        }

        /*
        var assets = AssetDatabase.LoadAllAssetsAtPath(oldSourcePath);
        for(var i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite sprite)
            {
              var instanceId=  assets[i].GetInstanceID();
              //Editor.c
            }
        }*/
    }

    private void ReSavePrefab()
    {
        string objPath = "Assets/Resources/Prefabs/Ground";
        DirectoryInfo directoryInfo = new DirectoryInfo(objPath);
        var files = directoryInfo.GetFiles("*.prefab");
         
        foreach(var file in files)
        {
            var path = $"{objPath}/{file.Name}";
            SavePrefab(path);
        }

        string objPath1 = "Assets/Resources/Prefabs/MapItem";
        DirectoryInfo directoryInfo1 = new DirectoryInfo(objPath1);
        var files1 = directoryInfo1.GetFiles("*.prefab");

        foreach (var file in files1)
        {
            var path = $"{objPath1}/{file.Name}";
            SavePrefab(path);
        }


        string objPath2 = "Assets/Resources/Prefabs/MapObj";
        DirectoryInfo directoryInfo2 = new DirectoryInfo(objPath2);
        var files2 = directoryInfo2.GetFiles("*.prefab");

        foreach (var file in files2)
        {
            var path = $"{objPath2}/{file.Name}";
            SavePrefab(path);
        }


        void SavePrefab(string path)
        {
            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var myLights = obj.GetComponentsInChildren<MyLight>(true);
            if (myLights != null && myLights.Length > 0)
            {
                for (int i = 0; i < myLights.Length; i++)
                {
                    var myLight = myLights[i];
                    var lerpColor = myLight.lerpColor;
                    var akeys = lerpColor.alphaKeys;
                    if (akeys.Length == 6)
                    {
                        akeys[1].time = 0.24f;
                        akeys[2].time = 0.25f;
                        akeys[3].time = 0.75f;
                        akeys[4].time = 0.76f;
                    }
                    lerpColor.alphaKeys = akeys;

                    var cKeys = lerpColor.colorKeys;
                    if (cKeys.Length == 6)
                    {
                        cKeys[1].time = 0.24f;
                        cKeys[2].time = 0.25f;
                        cKeys[3].time = 0.75f;
                        cKeys[4].time = 0.76f;
                    }
                    lerpColor.colorKeys = cKeys;

                    if (myLight.psCurve != null && myLight.psCurve.length > 0 && myLight.psCurve.keys.Length == 6)
                    {
                        var keys = myLight.psCurve.keys;
                        keys[1].time = 0.24f;
                        keys[2].time = 0.25f;
                        keys[3].time = 0.75f;
                        keys[4].time = 0.76f;
                        myLight.psCurve.keys = keys;
                    }
                }
                EditorUtility.SetDirty(obj);
                PrefabUtility.SaveAsPrefabAsset(obj, path);
            }
        }
    }
    private void ReSaveImage()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);
        var files=directoryInfo.GetFiles("*.png");
        Dictionary<string,int> fileOrders=new Dictionary<string,int>();
        foreach (var file in files)
        {
            var str = file.FullName.Split('.')[0];
            var strs=str.Split("_");
            var key = strs[0];
            if(!fileOrders.TryGetValue(key,out var order))
            {
                order = 0;
                fileOrders.Add(key, 0);
            }
            else
            {
                order++;
                fileOrders[key] = order;
            }
            int A = order / 3;
            int B = order % 3+1;
            string animationName="";
            switch (A)
            {
                case 0:
                    animationName = "下";
                    break;
                case 1:
                    animationName = "右";
                    break;
                case 2:
                    animationName = "上";
                    break;
            }

            File.Move(file.FullName, $"{strs[0]}_{animationName}_{B.ToString("00")}.png");

        }
    }
    private void AddReSaveImage()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);
        var files = directoryInfo.GetFiles("*.png");
        Dictionary<string,Dictionary<string,List<FileInfo>>> fileOrders = new Dictionary<string, Dictionary<string, List<FileInfo>>>();
        foreach (var file in files)
        {
            var str = file.Name.Split('.')[0];
            var strs = str.Split("_");
            var key = strs[0];
            var dir = strs[1];
            if(!fileOrders.TryGetValue(key,out var dirDic))
            {
                dirDic = new Dictionary<string, List<FileInfo>>();
                fileOrders.Add(key,dirDic);
            }
            if(!dirDic.TryGetValue(dir,out var fileInfos))
            {
                fileInfos = new List<FileInfo>();
                dirDic.Add(dir, fileInfos);
            }
            fileInfos.Add(file);
        }
        Dictionary<string, int> addDatas = new Dictionary<string, int>
        {
            {"通用动作",0},{"托举",0},{"坐",2},{"单手",2},
        };
        foreach(var d in fileOrders)
        {
            foreach(var add in addDatas)
            {
                if (add.Value > 0)
                {
                    foreach(var dir in d.Value)
                    {
                        var fileName = dir.Value[1].FullName;
                        var strs = fileName.Split("\\");
                        var path = fileName.Replace(strs[strs.Length-1], $"{d.Key}_{add.Key}_{dir.Key}_00.png");
                        File.Copy(fileName, path);
                    }
                }
                else
                {
                    foreach (var dir in d.Value)
                    {
                        for(int i = 0; i < dir.Value.Count; i++)
                        {
                            var fileName = dir.Value[i].FullName;
                            var strs = fileName.Split("\\");
                            var path = fileName.Replace(strs[strs.Length - 1], $"{d.Key}_{add.Key}_{dir.Key}_{(i+1).ToString("00")}.png");
                            File.Copy(fileName, path,true);
                        }
                       
                    }
                }
            }
        }
    }
    private void InitPlantAnimation()
    {
        string plantDataPath = "Data/PlantData";
        string plantAnimationDir = "Assets/Animation/Object/";
        string sourcePath = "Assets/Texture/Farm/Farm.png";
        var sources = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
        Dictionary<string, List<Sprite>> plantSources = new Dictionary<string, List<Sprite>>();
        foreach (var source in sources)
        {
            if (source is Sprite sprite)
            {
                string key = sprite.name.Split('_')[0];
                if (!plantSources.TryGetValue(key, out var sprites))
                {
                    sprites = new List<Sprite>();
                    plantSources.Add(key, sprites);
                }
                sprites.Add(sprite);
            }
        }

        try
        {
            AssetDatabase.StartAssetEditing();
            var objs = Resources.LoadAll(plantDataPath);

            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                if (obj is PlantData plantData)
                {
                    List<Sprite> sprites = plantSources[plantData.plantName];

                    if (plantData.plantName == "玉米")
                    {
                        continue;
                    }

                    AnimatorController animatorController = new AnimatorController();
                    animatorController.AddLayer("Base Layer");

                    string animationDirPath = $"{plantAnimationDir}{plantData.plantName}";
                    DirectoryInfo directoryInfo = new DirectoryInfo(animationDirPath);
                    var files = directoryInfo.GetFiles("*.anim");
                    foreach (var file in files)
                    {
                        AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animationDirPath}/{file.Name}");
                        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
                        foreach (var binding in bindings)
                        {
                            if (binding.propertyName != "m_Sprite")
                            {
                                continue;
                            }
                            var keyframes = AnimationUtility.GetObjectReferenceCurve(animationClip, binding);
                            for (int j = 0; j < keyframes.Length; j++)
                            {
                                var keyframe = keyframes[j];
                                Sprite sprite = keyframe.value as Sprite;
                                var strs = sprite.name.Split("_");
                                string keyName = $"{plantData.plantName}_{strs[strs.Length - 1]}";
                                keyframe.value = sprites.Find(s => s.name == keyName);
                                keyframes[j] = keyframe;
                            }
                            AnimationUtility.SetObjectReferenceCurve(animationClip, binding, keyframes);
                        }

                        AnimatorState animatorState = new AnimatorState
                        {
                            name=animationClip.name,
                            motion = animationClip
                        };
                        animatorController.layers[0].stateMachine.AddState(animatorState, Vector3.zero);
                    }
                    AssetDatabase.CreateAsset(animatorController, $"{animationDirPath}/{plantData.plantName}.controller");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private void CreatPlantMapItemData()
    {
        string mapItemDataPath = "Assets/Resources/Data/MapItemData/";

        string plantDataPath = "Data/PlantData";
        string itemAnimationDataPath = "Assets/Resources/Data/ItemAnimationData/";
        string plantAnimationData = "Assets/Resources/Data/ItemAnimationData/8019.asset";

        ItemAnimationData itemAnimationData = AssetDatabase.LoadAssetAtPath<ItemAnimationData>(plantAnimationData);

        try
        {
            AssetDatabase.StartAssetEditing();
            var objs = Resources.LoadAll(plantDataPath);
            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                if (obj is PlantData plantData)
                {
                    ItemAnimationData animationData = new ItemAnimationData
                    {
                        name = plantData.id.ToString(),
                        animationStateDatas = new List<AnimationStateData>(),
                        animationStateDataDic = new ItemAnimationDictionary(),
                    };
                    for (int j = 0; j < itemAnimationData.animationStateDatas.Count; j++)
                    {
                        AnimationStateData animationStateData = itemAnimationData.animationStateDatas[j];

                        AnimationStateData newStateData = new AnimationStateData
                        {
                            key = animationStateData.key,
                            stateName = animationStateData.stateName,
                            clips = new List<AnimationClip>(),
                        };
                        for (int index = 0; index < animationStateData.clips.Count; index++)
                        {
                            var clip = animationStateData.clips[index];
                            var clipPath = AssetDatabase.GetAssetPath(clip);
                            var strs = clipPath.Split('/');

                            clipPath = clipPath.Replace(strs[strs.Length-2], plantData.plantName);
                            var newClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                            newStateData.clips.Add(newClip);
                        }
                        animationData.animationStateDatas.Add(newStateData);
                    }
                    animationData.InitDic();

                    AssetDatabase.CreateAsset(animationData, $"{itemAnimationDataPath}/{plantData.mapItem}.asset");

                    MapItemData mapItemData = new MapItemData
                    {
                        id = plantData.mapItem,
                        itemName = plantData.plantName,
                        objName = plantData.plantName,
                        itemObj = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Prefabs/MapItem/{plantData.plantName}.prefab")
                    };
                    AssetDatabase.CreateAsset(mapItemData, $"{mapItemDataPath}{plantData.mapItem}.asset");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    private void CreatPlantMapItemObj()
    {
        string plantDataPath = "Data/PlantData";
        string plantObjPath = "Assets/Resources/Prefabs/MapItem/";

        string plantAnimationDir = "Assets/Animation/Object/";
        string plantAnimationDir1 = "Assets/Animation/Object/玉米";

        string sourcePath = "Assets/Texture/Farm/Farm.png";
        var sources = AssetDatabase.LoadAllAssetsAtPath(sourcePath);
        Dictionary<string, List<Sprite>> plantSources = new Dictionary<string, List<Sprite>>();
        foreach (var source in sources)
        {
            if (source is Sprite sprite)
            {
                string key = sprite.name.Split('_')[0];
                if (!plantSources.TryGetValue(key, out var sprites))
                {
                    sprites = new List<Sprite>();
                    plantSources.Add(key, sprites);
                }
                sprites.Add(sprite);
            }
        }

        GameObject plantPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/MapItem/玉米.prefab");
        try
        {
            AssetDatabase.StartAssetEditing();
            var objs = Resources.LoadAll(plantDataPath);

            DirectoryInfo directoryInfo = new DirectoryInfo(plantAnimationDir1);
            var files = directoryInfo.GetFiles("*.anim");

            for (int i = 0; i < objs.Length; i++)
            {
                var obj = objs[i];
                if (obj is PlantData plantData)
                {
                    if (plantData.plantName == "玉米")
                    {
                        continue;
                    }
                    GameObject plantObj = GameObject.Instantiate(plantPrefab);
                    if (plantSources.TryGetValue(plantData.plantName, out var sprites))
                    {
                        plantObj.transform.GetChild(0).GetChild(0).GetComponent<SpriteRenderer>().sprite = sprites[sprites.Count - 2];
                    }
                    plantObj.name = plantData.plantName;
                    PrefabUtility.SaveAsPrefabAsset(plantObj, $"{plantObjPath}{plantData.plantName}.prefab");
                    GameObject.DestroyImmediate(plantObj);

                    string animationDir = $"{plantAnimationDir}{plantData.plantName}";
                    if (Directory.Exists(animationDir))
                    {
                        Directory.Delete(animationDir, true);
                    }
                    Directory.CreateDirectory(animationDir);

                    foreach (var file in files)
                    {
                        string newFilePath = file.FullName.Replace("玉米", plantData.plantName);

                        File.Copy(file.FullName, newFilePath);
                    }
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    public string sourcePath;
    public string animationPath;

}