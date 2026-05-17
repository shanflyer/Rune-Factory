#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.U2D.Animation;
using UnityEngine.U2D;
using System.Collections.Generic;
using System.IO;
using Anima2D;
using System.Linq;
using System.Net.WebSockets;
using Unity.Collections;
using UnityEngine.Rendering;
using BoneWeight = Anima2D.BoneWeight;
using UnityEditor.Animations;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

public class PSBToAnima2DConverter : EditorWindow
{
    private GameObject _psbPrefab;
    private string _outputFolder = "Assets/Texture/NewPsbTexture";
    private Material newMat = null;
    private string animationDir = "Assets/Animation/NewAnimation";
    private string copyAnimationDir = "Assets/Animation/NewAnimation/主角";
   
    [MenuItem("工具/PSB To Anima2D Converter")]
    private static void ShowWindow()
    {
        var window = GetWindow<PSBToAnima2DConverter>();
        window.titleContent = new GUIContent("PSB → Anima2D");
        window.minSize = new Vector2(500, 200);
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("PSB Prefab → Anima2D 资产转换器", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("New Mat", GUILayout.Width(80));
        newMat = (Material)EditorGUILayout.ObjectField(newMat, typeof(Material), false);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("PSB Prefab", GUILayout.Width(80));
        _psbPrefab = (GameObject)EditorGUILayout.ObjectField(_psbPrefab, typeof(GameObject), false);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Copy AnimationPath", GUILayout.Width(80));
        copyAnimationDir = EditorGUILayout.TextField(copyAnimationDir);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("输出动画目录", GUILayout.Width(80));
        animationDir= EditorGUILayout.TextField(animationDir);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("输出目录", GUILayout.Width(80));
        _outputFolder = EditorGUILayout.TextField(_outputFolder);
        if (GUILayout.Button("📂", GUILayout.Width(28)))
        {
            string sel = EditorUtility.OpenFolderPanel("选择输出目录", Application.dataPath, "");
            if (!string.IsNullOrEmpty(sel))
            {
                if (sel.StartsWith(Application.dataPath))
                {
                    _outputFolder = "Assets" + sel.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog("路径错误",
                        "请选择位于本项目 Assets 目录下的文件夹。", "OK");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        if (GUILayout.Button("移动资源", GUILayout.Height(30)))
        {
            if (_psbPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定一个 PSB Prefab。", "OK");
            }
            else
            {
                var outputFolder = $"{_outputFolder}/{_psbPrefab.name.Split(".")[0]}";
                if (!AssetDatabase.IsValidFolder(outputFolder))
                {

                }
                else
                {
                    AssetDatabase.DeleteAsset(outputFolder);
                }
                Directory.CreateDirectory(Application.dataPath + "/" + outputFolder.Replace("Assets/", ""));
                AssetDatabase.Refresh();
                CopySource(_psbPrefab, outputFolder);
            }
        }
        if (GUILayout.Button("生成mesh", GUILayout.Height(30)))
        {
            if (_psbPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定一个 PSB Prefab。", "OK");
            }
            else
            {
                var outputFolder = $"{_outputFolder}/{_psbPrefab.name.Split(".")[0]}";
                CreateSpriteMesh(outputFolder);
            }
        }


        if (GUILayout.Button("Convert", GUILayout.Height(30)))
        {
            if (_psbPrefab == null)
            {
                EditorUtility.DisplayDialog("错误", "请先指定一个 PSB Prefab。", "OK");
            }
            else
            {
              var  outputFolder = $"{_outputFolder}/{_psbPrefab.name.Split(".")[0]}";  
                ConvertPSBToAnima2D(_psbPrefab, outputFolder);
            }
        }

        if (GUILayout.Button("绑定骨骼",GUILayout.Height(30)))
        {
            var outputFolder = $"{_outputFolder}/{_psbPrefab.name.Split(".")[0]}/{_psbPrefab.name.Split(".")[0]}.prefab";
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(outputFolder);
            SetBone(gameObject);
        }
        if(GUILayout.Button("动画初始化", GUILayout.Height(30)))
        {
            InitOverrideAnimation();
        }
    }

    AnimatorController copyController;
    AnimationClip[] copyClips;

    AnimatorController CopyController
    {
        get
        {
            if(copyController == null)
            {
                var strs= copyAnimationDir.Split('/');
                copyController = AssetDatabase.LoadAssetAtPath<AnimatorController>($"{copyAnimationDir}/{strs[strs.Length-1]}.controller");

                var dir = new DirectoryInfo(copyAnimationDir);
                var clipPs = dir.GetFiles("*.anim");
                copyClips = new AnimationClip[clipPs.Length];
                for(int i = 0; i < clipPs.Length; i++)
                {
                    AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{copyAnimationDir}/{clipPs[i].Name}");
                    copyClips[i] = clip;
                }
            }
            return copyController;
        }
    }
    AnimationClip[] CopyClips;
     
    void InitOverrideAnimation()
    {
        DirectoryInfo parentDir = new DirectoryInfo(animationDir);
        var dirs = parentDir.GetDirectories();

        DirectoryInfo characterDir = null;
        foreach(var dir in dirs )
        {
            if (dir.Name == _psbPrefab.name)
            {
                characterDir = dir;
                break;
            }
        }
        if (characterDir == null)
        { 
            Directory.CreateDirectory($"{parentDir}/{_psbPrefab.name}");
            AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController(CopyController);
            AssetDatabase.CreateAsset(animatorOverrideController, $"{animationDir}/{_psbPrefab.name}/{_psbPrefab.name}.overrideController");

            string[] oldAssetPath = new string[copyClips.Length];
            string[] newAssetPath = new string[copyClips.Length];
            for (int i = 0; i < copyClips.Length; i++)
            {
                oldAssetPath[i] = AssetDatabase.GetAssetPath(copyClips[i]);
                var strs = oldAssetPath[i].Split('/');
                newAssetPath[i] = oldAssetPath[i].Replace(strs[strs.Length - 2], _psbPrefab.name);
            }
            AssetDatabase.CopyAssets(oldAssetPath, newAssetPath);

            AssetDatabase.Refresh();
            
            AssetDatabase.Refresh();
            characterDir = new DirectoryInfo($"{parentDir}/{_psbPrefab.name}");

        }

        var animations = characterDir.GetFiles("*.anim");
        var controllerFile = characterDir.GetFiles("*.overrideController");

        Dictionary<string, AnimationClip> clipDic = new Dictionary<string, AnimationClip>();
        foreach (var animationP in animations)
        {
            AnimationClip animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{animationDir}/{characterDir.Name}/{animationP.Name}");
            clipDic.Add(animationClip.name, animationClip);
        }

        var overrideController = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>($"{animationDir}/{characterDir.Name}/{controllerFile[0].Name}");
        List<KeyValuePair<AnimationClip, AnimationClip>> animationClips = new List<KeyValuePair<AnimationClip, AnimationClip>>();
        overrideController.GetOverrides(animationClips);
        for (int i = 0; i < animationClips.Count; i++)
        {
            var animationKV = animationClips[i];
            if (clipDic.TryGetValue(animationKV.Key.name, out var animationClip))
            {
                animationClips[i] = new KeyValuePair<AnimationClip, AnimationClip>(animationKV.Key, animationClip);
            }
        }
        overrideController.ApplyOverrides(animationClips);
    }

    void CreateSpriteMesh(string outputFolder)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
        var files = directoryInfo.GetFiles("*.png");
        foreach (var file in files)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"{outputFolder}/{file.Name}");
            SpriteMeshUtils.CreateSpriteMesh(tex);
            break;
        }
    }
    void CopySource(GameObject psbPrefab, string outputFolder)
    {
        string prefabPath = AssetDatabase.GetAssetPath(psbPrefab);
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[PSB→Anima2D] 找不到 PSB Prefab 的资源路径。");
            return;
        }
        var item = CommonTool.CopyPSBSource(prefabPath, outputFolder);
        try
        {
            AssetDatabase.StartAssetEditing();

            var assets = AssetDatabase.LoadAllAssetsAtPath(item.Item1);
            Dictionary<Rect, Sprite> newSprites = new Dictionary<Rect, Sprite>();

            Texture2D newTex = null;
            Sprite singleSprite = null;
            foreach (var asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    singleSprite = sprite;
                    newSprites.Add(sprite.rect, sprite);
                }
                else if (asset is Texture2D texture)
                {
                    newTex = texture;
                }
            }


            // 载入 PSB Prefab 内容
            GameObject rootContents = item.Item2;
            if (rootContents == null)
            {
                Debug.LogError("[PSB→Anima2D] 无法加载 PSB Prefab 内容。");
                return;
            }

            string prefabName = psbPrefab.name;
            // 在输出目录下创建子文件夹
            string subFolder = outputFolder;

            Material psbMaterial = new Material(newMat);

            psbMaterial.SetTexture("_MainTex", newTex);
            SecondarySpriteTexture[] secondarySpriteTextures = new SecondarySpriteTexture[singleSprite.GetSecondaryTextureCount()];
            if (secondarySpriteTextures.Length != 0)
            {
                singleSprite.GetSecondaryTextures(secondarySpriteTextures);

                for (int i = 0; i < secondarySpriteTextures.Length; i++)
                {
                    psbMaterial.SetTexture(secondarySpriteTextures[i].name, secondarySpriteTextures[i].texture);
                }

            }


            string matPath = Path.Combine(subFolder, prefabName + "_Mat.mat").Replace("\\", "/");
            // 删除旧材质
            if (AssetDatabase.LoadAssetAtPath<Material>(matPath) != null)
            {
                AssetDatabase.DeleteAsset(matPath);
            }
            AssetDatabase.CreateAsset(psbMaterial, matPath);
            AssetDatabase.SaveAssets();

              

            Selection.activeObject = newTex;
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    void SetBone(GameObject ObjPrefab)
    {

        GameObject obj = Instantiate(ObjPrefab);
        var spriteInstances = obj.GetComponentsInChildren<SpriteMeshInstance>(true);

        var m_SpriteMeshCache = ScriptableObject.CreateInstance<SpriteMeshCache>();
        foreach(var spriteInstance in spriteInstances)
        {
            m_SpriteMeshCache.Clear("");
            m_SpriteMeshCache.SetSpriteMesh(spriteInstance.spriteMesh,spriteInstance);
            m_SpriteMeshCache.BindBones();
            m_SpriteMeshCache.CalculateAutomaticWeights();
            m_SpriteMeshCache.ApplyChanges();
        }
       
      
        PrefabUtility.SaveAsPrefabAsset(obj, AssetDatabase.GetAssetPath(ObjPrefab));
        DestroyImmediate(obj);

    }
    private void ConvertPSBToAnima2D(GameObject psbPrefab, string outputFolder)
    {
       

        string prefabPath = AssetDatabase.GetAssetPath(psbPrefab);
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogError("[PSB→Anima2D] 找不到 PSB Prefab 的资源路径。");
            return;
        }
       var item= CommonTool.CopyPSBObj(prefabPath, outputFolder);

        try
        {
            AssetDatabase.StartAssetEditing();
             
            
  
            // 载入 PSB Prefab 内容
            GameObject rootContents = item;
            if (rootContents == null)
            {
                Debug.LogError("[PSB→Anima2D] 无法加载 PSB Prefab 内容。");
                return;
            }

             

            // ----- 开始构建 Anima2D 结构 -----
            GameObject animaRootGO = Instantiate(rootContents);

            // 1. 收集所有骨骼 Transform 并排序（保证父先出）
            var spriteSkins = animaRootGO.GetComponentsInChildren<SpriteSkin>(true);

            List<Transform> boneRoots = new List<Transform>();
            foreach (Transform child in animaRootGO.transform)
            {
                if (child.name.Contains("Root"))
                {
                    boneRoots.Add(child);
                }
            }
            Dictionary<string, Bone2D> boneDic = new Dictionary<string, Bone2D>();

            Dictionary<Bone2D, Bone2D> boneRootMap = new Dictionary<Bone2D, Bone2D>();
            foreach (var bone in boneRoots)
            {
                // GameObject newBone = Instantiate(bone.gameObject, animaRootGO.transform);
                // newBone.name = bone.name;
                AddBone2D(bone.transform,null);

                List<Transform> bones = new List<Transform>();
                for(int i = bone.childCount-1; i >=0; i--)
                {
                    Transform child = bone.transform.GetChild(i);
                    child.SetParent(bone.parent, true);
                    bones.Add(child);
                }
                bone.localRotation= Quaternion.identity;
                foreach(var child in bones)
                {
                    child.SetParent(bone, true);
                }
                if (bone.gameObject.TryGetComponent<Bone2D>(out var bone2D))
                {
                    bone2D.child = null;
                    bone2D.localLength = 0f;
                }

                
            }

            void AddBone2D(Transform child,Bone2D root)
            {
                Bone2D newBone = child.gameObject.AddComponent<Bone2D>();
                boneDic.Add(child.name, newBone);
                if (root == null)
                {
                    root = newBone;
                }
                else
                {
                    boneRootMap.Add(newBone, root);
                }
                if (child.childCount == 0)
                {
                    newBone.localLength = 0.0325f;
                }
                else
                {
                    newBone.localLength = Vector2.Distance(Vector2.zero, child.GetChild(0).localPosition);
                    foreach (Transform tr in child)
                    {
                        AddBone2D(tr,root);
                    }
                }
                /*
                if (child.parent != null && child.parent.gameObject.TryGetComponent<Bone2D>(out var parentBone) && parentBone.child == null)
                {
                    parentBone.child = newBone;
                }*/

            }

            Dictionary<Rect, Sprite> newSprites = new Dictionary<Rect, Sprite>();
            Dictionary<Sprite, SpriteMesh> spriteMeshDic = new Dictionary<Sprite, SpriteMesh>();
            DirectoryInfo directoryInfo = new DirectoryInfo(outputFolder);
            var files = directoryInfo.GetFiles("*.asset");
            foreach(var file in files)
            {
                var filePath = $"{outputFolder}/{file.Name}";
                var obj = AssetDatabase.LoadAssetAtPath<SpriteMesh>(filePath);
                spriteMeshDic.Add(obj.sprite, obj);
                newSprites.Add(obj.sprite.rect, obj.sprite);
            }
            string matPath = $"{outputFolder}/{directoryInfo.Name}_Mat.mat";
            var psbMaterial = AssetDatabase.LoadAssetAtPath<Material>(matPath);

            // 6. 为每个 SpriteSkin 生成 SpriteMesh + Mesh、SpriteMeshInstance，并创建/覆盖 SkinnedMeshRenderer
            foreach (var skin in spriteSkins)
            {
                // if (skin.sprite == null) continue;

                Transform origSkinT = skin.transform;


                Sprite usedSprite = skin.GetComponent<SpriteRenderer>().sprite;
                if (usedSprite == null)
                    continue;

                if (newSprites.TryGetValue(usedSprite.rect, out var newSprite))
                {
                    // 获取顶点、uv、索引、权重、BindPose
                    Vector3[] positions = CopyNativeSlice(usedSprite.GetVertexAttribute<Vector3>(VertexAttribute.Position));
                    Vector2[] uvs = usedSprite.uv;
                    ushort[] triUshorts = usedSprite.triangles;
                    var weights = CopyNativeSlice(usedSprite.GetVertexAttribute<UnityEngine.BoneWeight>(VertexAttribute.BlendWeight));
                    Matrix4x4[] bindposes = CopyNativeArray(usedSprite.GetBindPoses());

                    // 转成 int[]
                    int[] triangles = new int[triUshorts.Length];
                    for (int i = 0; i < triUshorts.Length; i++)
                        triangles[i] = triUshorts[i];

                    
                    if(spriteMeshDic.TryGetValue(newSprite,out var spriteMeshAsset)) { }
                  //  var spriteMeshAsset = SpriteMeshUtils.CreateSpriteMesh(newSprite);
                     

                    Transform existing = skin.transform;
                    GameObject smiGO = existing.gameObject;
                    // 删除旧组件
                    var oldSMI = smiGO.GetComponent<SpriteMeshInstance>();
                    if (oldSMI) Undo.DestroyObjectImmediate(oldSMI);
                    var oldFilter = smiGO.GetComponent<MeshFilter>();
                    if (oldFilter) Undo.DestroyObjectImmediate(oldFilter);
                    var oldRend = smiGO.GetComponent<MeshRenderer>();
                    if (oldRend) Undo.DestroyObjectImmediate(oldRend);


                    var oldSkinned = smiGO.GetComponent<SkinnedMeshRenderer>();
                   // if (oldSkinned) Undo.DestroyObjectImmediate(oldSkinned);



                    smiGO.transform.localPosition = origSkinT.localPosition;
                    smiGO.transform.localRotation = origSkinT.localRotation;
                    smiGO.transform.localScale = origSkinT.localScale;

                    SpriteMeshInstance smi = smiGO.AddComponent<SpriteMeshInstance>(); 
                    smi.spriteMesh = spriteMeshAsset;
                    smi.sharedMaterial = psbMaterial;
                    var sr = skin.GetComponent<SpriteRenderer>();
                    smi.sortingLayerID = sr.sortingLayerID;
                    smi.sortingOrder = sr.sortingOrder;
                    smi.color = sr.color;
                    var animaBones = new List<Bone2D>();
                    bool rootInit = false;
                    foreach (var bone in skin.boneTransforms)
                    {
                        var bone2D = bone.GetComponent<Bone2D>();
                        if (!rootInit)
                        {
                            rootInit = true;
                            if(boneRootMap.TryGetValue(bone2D,out var root))
                            {
                                animaBones.Add(root);
                            }
                        }
                        animaBones.Add(bone2D);
                    }
                    smi.bones = animaBones;

                    // 创建/覆盖 SkinnedMeshRenderer
                    var smr = smiGO.GetComponent<SkinnedMeshRenderer>();
                    if (smr == null)
                    {
                        smr = Undo.AddComponent<SkinnedMeshRenderer>(smiGO);
                    }
                    smr.sharedMesh = spriteMeshAsset.sharedMesh;
                    smr.sharedMaterial = psbMaterial;
                    smr.bones = animaBones.Where(x => x != null).Select(x => x.transform).ToArray();
                    if (animaBones.Count > 0 && animaBones[0] != null)
                    {
                        smr.rootBone = animaBones[0].transform;
                    }

                    Undo.DestroyObjectImmediate(skin);
                    Undo.DestroyObjectImmediate(sr);
                }


            }

            // 7. 保存/覆盖 最终 Prefab
            string newPrefabPath = $"{outputFolder}/{directoryInfo.Name}.Prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(newPrefabPath) != null)
            {
                AssetDatabase.DeleteAsset(newPrefabPath);
            }
            PrefabUtility.SaveAsPrefabAsset(animaRootGO, newPrefabPath);

            DestroyImmediate(animaRootGO);

            AssetDatabase.Refresh();
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
      
    }

     

    /// <summary>
    /// 递归复制原始 Prefab 的 Transform 层级，只创建非骨骼节点的空 GameObject，
    /// 骨骼节点留待后续 Bone2D 创建时插入。hierarchyMap 用于映射原节点到新节点。
    /// </summary>
    private void CreateEmptyHierarchy(
        Transform original,
        Transform parentNew,
        List<Transform> allBoneTransforms,
        Dictionary<Transform, Transform> hierarchyMap
    )
    {
        foreach (Transform child in original)
        {
            if (allBoneTransforms.Contains(child))
            {
                hierarchyMap[child] = null;
                CreateEmptyHierarchy(child, parentNew, allBoneTransforms, hierarchyMap);
            }
            else
            {
                GameObject newGO = new GameObject(child.name);
                newGO.transform.SetParent(parentNew, false);
                newGO.transform.localPosition = child.localPosition;
                newGO.transform.localRotation = child.localRotation;
                newGO.transform.localScale = child.localScale;

                hierarchyMap[child] = newGO.transform;
                CreateEmptyHierarchy(child, newGO.transform, allBoneTransforms, hierarchyMap);
            }
        }
    }

    /// <summary>
    /// 从 NativeSlice<T> 中拷贝数据到数组
    /// </summary>
    static T[] CopyNativeSlice<T>(NativeSlice<T> slice) where T : struct
    {
        T[] arr = new T[slice.Length];
        for (int i = 0; i < slice.Length; i++)
            arr[i] = slice[i];
        return arr;
    }

    static T[] CopyNativeArray<T>(NativeArray<T> array) where T : struct
    {
        T[] arr = new T[array.Length];
        for (int i = 0; i < array.Length; i++)
            arr[i] = array[i];
        return arr;
    }

    /// <summary>
    /// 获取 Transform 在层级中的深度
    /// </summary>
    private static int GetTransformDepth(Transform t)
    {
        int depth = 0;
        while (t.parent != null)
        {
            depth++;
            t = t.parent;
        }
        return depth;
    }
}
#endif
