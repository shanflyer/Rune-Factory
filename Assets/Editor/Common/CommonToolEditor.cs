using Codice.CM.Client.Differences;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class CommonToolEditor:MyEditor
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
        if (GUILayout.Button("转换坐标"))
        {
            try
            {
                AssetDatabase.StartAssetEditing();
                OldItemCellToNew();
                OldMapCellToNew();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
        sourcePath=EditorGUILayout.TextField("源路径",sourcePath);
        animationPath= EditorGUILayout.TextField("输出路径", animationPath);
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

    public string sourcePath;
    public string animationPath;

    void OldMapCellToNew()
    {

        string mapParentPath = "Assets/Resources/Data/MapRoomData/";
        DirectoryInfo mapDir = new DirectoryInfo(mapParentPath);
        var files = mapDir.GetFiles("*.asset");
        foreach (var file in files)
        {
            string filePath = $"{mapParentPath}{file.Name}";
            MapRoomData mapRoomData=AssetDatabase.LoadAssetAtPath<MapRoomData>(filePath);
            List<MapCellData> mapCells = new List<MapCellData>(); 
            foreach(var cell in mapRoomData.mapCells)
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        MapCellData mapCell = new MapCellData
                        {
                            coordinate = cell.coordinate * 2 + new int2(x, y),
                            isWalkable = cell.isWalkable
                        };
                        mapCells.Add(mapCell);
                    }
                }
            }
            mapRoomData.mapCells = mapCells;

            for(int i = 0; i < mapRoomData.mapItems.Count; i++)
            {
                var mapItem = mapRoomData.mapItems[i];
                mapItem.coordinate = mapItem.coordinate * 2;
                mapRoomData.mapItems[i] = mapItem;
            }

            mapRoomData.startCoordinate *= 2;
            mapRoomData.endCoordinate *= 2;

            EditorUtility.SetDirty(mapRoomData);
            AssetDatabase.SaveAssets();
        }
    }

    void OldItemCellToNew()
    {
        string itemParentPath = "Assets/Resources/Data/MapItemData/";
        DirectoryInfo itemDir = new DirectoryInfo(itemParentPath);
        var files = itemDir.GetFiles("*.asset");
        foreach(var file in files)
        {
            string filePath = $"{itemParentPath}{file.Name}";
            MapItemData itemData=AssetDatabase.LoadAssetAtPath<MapItemData>(filePath);

            if (itemData.colliderCells != null)
            {
                List<int2> colliderCells = new List<int2>();
                foreach (var cell in itemData.colliderCells)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            colliderCells.Add(cell * 2 + new int2(x, y));
                        }
                    }
                }

                itemData.colliderCells = colliderCells.ToArray();

            }
                

            if (itemData.triggerCells != null)
            {
                List<int2> triggerCells = new List<int2>();
                foreach (var cell in itemData.triggerCells)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            triggerCells.Add(cell * 2 + new int2(x, y));
                        }
                    }
                }
                itemData.triggerCells = triggerCells.ToArray();
            }
               

            if (itemData.playerTriggerCells != null)
            {
                List<int2> playerTriggerCells = new List<int2>();
                foreach (var cell in itemData.playerTriggerCells)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            playerTriggerCells.Add(cell * 2 + new int2(x, y));
                        }
                    }
                }
                itemData.playerTriggerCells = playerTriggerCells.ToArray();
            }
           

            EditorUtility.SetDirty(itemData);
            AssetDatabase.SaveAssets();
        }
    }
}
