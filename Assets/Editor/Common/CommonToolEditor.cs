using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

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
    private void ReSaveImage()
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(sourcePath);
        var files=directoryInfo.GetFiles("*.png");
        Dictionary<string,int> fileOrders=new Dictionary<string,int>();
        foreach (var file in files)
        {
            var str = file.Name.Split('.')[0];
            var strs=str.Split("_");
            var key = str.Replace(strs[strs.Length - 1], "");
            if(!fileOrders.TryGetValue(key,out var order))
            {
                order = 1;
                fileOrders.Add(key, 1);
            }
            else
            {
                order++;
                fileOrders[key] = order;
            }


            File.Move(file.FullName, file.FullName.Replace(strs[strs.Length - 1], order.ToString("00")));

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
        string plantAnimationData = "Assets/Resources/Data/ItemAnimationData/8000.asset";

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