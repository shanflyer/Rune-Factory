using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif
 
public class SpriteBonePoseController : MonoBehaviour
{
    public SpriteBonePose spriteBone;
    public FrameAnimationData frameAnimationData;
    public AnimationClip animationClip;
    HashSet<string> SpecialParts = new HashSet<string>
    {
        "前", "左", "后", "鱼线", "鱼竿", "Shadow", "Other"
    };
#if UNITY_EDITOR
    public void TransformToPoseData()
    {
        if (spriteBone == null)
        {
            return;
        }
        spriteBone.Clear();
        spriteBone.DisplayGroup = null;
        foreach(Transform group in transform)
        {
            if (SpecialParts.Contains(group.name))
            {
                continue;
            }
            if (!group.gameObject.activeSelf)
            {
                spriteBone.HideGroup.Add(group.name);
                continue;
            }
            else
            {
                spriteBone.DisplayGroup = group.name;
            }
           // if (!string.IsNullOrEmpty(spriteBone.DisplayGroup))
            {
          //      group.gameObject.SetActive(false);
         //       return;
            } 
            var children = group.GetComponentsInChildren<Transform>(true);
          
            for (int i = 0; i < children.Length; i++)
            {
                Transform child = children[i];
                if (child == group)
                {
                    continue;
                }
                BonePose bonePose = new BonePose(child,GetPartPath(child,transform));
                spriteBone.DisplayBonePoses.Add(bonePose);
            }
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(spriteBone);
        AssetDatabase.SaveAssets();
#endif
    }
   
    string[] propertyNames = new string[]
    {
        "m_LocalPosition.x","m_LocalPosition.y","m_LocalPosition.z", "localEulerAnglesRaw.z",
        "m_LocalScale.x","m_LocalScale.y",
    };
    public void RecordAnimation()
    {
        float perFameTime = 1.0f / frameAnimationData.fps;
        
        string path = AssetDatabase.GetAssetPath(animationClip);
        Dictionary<string,Dictionary<string, List<Vector2>>> AnimationDatas = new Dictionary<string, Dictionary<string, List<Vector2>>>();
        for(int i=0;i< frameAnimationData.bones.Count; i++)
        {
            var bone=frameAnimationData.bones[i];
            float nowTime = perFameTime * i;

            for(int j = 0; j < bone.DisplayBonePoses.Count; j++)
            {
                var bonePose = bone.DisplayBonePoses[j];
                if (!AnimationDatas.TryGetValue($"{bonePose.name}", out var animationDatas))
                {
                    animationDatas = new Dictionary<string, List<Vector2>>();
                    AnimationDatas.Add(bonePose.name, animationDatas);
                }


                foreach (var propertyName in propertyNames)
                {
                    if (!animationDatas.TryGetValue(propertyName, out var animations))
                    {
                        animations = new List<Vector2>();
                        animationDatas.Add(propertyName, animations);
                    }

                    

                    switch (propertyName)
                    {
                        case "m_LocalPosition.x":
                            animations.Add(new Vector2(nowTime,bonePose.position.x));
                            break;
                        case "m_LocalPosition.y":
                            animations.Add(new Vector2(nowTime, bonePose.position.y));
                            break;
                        case "m_LocalPosition.z":
                            animations.Add(new Vector2(nowTime, bonePose.position.z));
                            break;

                        case "localEulerAnglesRaw.z":
                            animations.Add(new Vector2(nowTime, bonePose.angle.z));
                            break;
                        case "m_LocalScale.x":
                            animations.Add(new Vector2(nowTime, bonePose.scale.x));
                            break;
                        case "m_LocalScale.y":
                            animations.Add(new Vector2(nowTime, bonePose.scale.y));
                            break;
                        case "m_LocalScale.z":
                            animations.Add(new Vector2(nowTime, bonePose.scale.z));
                            break;
                    }
                     
                }
            }
        }

        foreach(var animationDataDic in AnimationDatas)
        {
            foreach(var animationData in animationDataDic.Value)
            {
                EditorCurveBinding editorCurveBinding = new EditorCurveBinding
                {
                    path = animationDataDic.Key,
                    propertyName = animationData.Key,
                    type = typeof(Transform),
                };
                List<Keyframe> keyframes = new List<Keyframe>();
                foreach(var data in animationData.Value)
                {
                    Keyframe keyframe = new Keyframe
                    {
                        time = data.x,
                        value = data.y,
                        inTangent = float.PositiveInfinity,
                        outTangent = float.PositiveInfinity,
                        inWeight = 0
                    };
                    keyframes.Add(keyframe);
                }
                AnimationCurve animationCurve = new AnimationCurve
                {
                    keys = keyframes.ToArray(),
                };
                //animationClip.SetCurve(editorCurveBinding.path, typeof(Transform),editorCurveBinding.)
                AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, animationCurve);
            }
        }

        EditorUtility.SetDirty(animationClip);
        AssetDatabase.SaveAssets();
    }
    public void PoseDataToTransform()
    {

        if (spriteBone == null|| string.IsNullOrEmpty(spriteBone.DisplayGroup))
        {
            return;
        }
        var children = transform.GetComponentsInChildren<Transform>(true);
        Dictionary<string, Transform> transformDic = new Dictionary<string, Transform>();
        for (int i = 0; i < children.Length; i++)
        {
            transformDic.Add(children[i].name, children[i]);
        }
        if (transformDic.TryGetValue(spriteBone.DisplayGroup, out var displayChild))
        {
            displayChild.gameObject.SetActive(true);
        }
        for (int i = 0; i < spriteBone.HideGroup.Count; i++)
        {
            if (transformDic.TryGetValue(spriteBone.HideGroup[i],out var child))
            {
                child.gameObject.SetActive(false);
            }
        }

 
        try
        {   
            for (int i = 0; i < spriteBone.DisplayBonePoses.Count; i++)
            {
                var bonePose = spriteBone.DisplayBonePoses[i];
                var child = transform.Find(bonePose.name);
                if (child!=null)
                {
                    Undo.RecordObject(child, "Load Pose");
                    child.localEulerAngles = bonePose.angle;
                    child.localPosition = bonePose.position;
                    child.localScale = bonePose.scale; 
                }
            }
            for (int i = 0; i < spriteBone.LayerParts.Count; i++)
            {
                var layerPart = spriteBone.LayerParts[i];
                var child = transform.Find(layerPart.name);
                if (child.TryGetComponent<SpriteRenderer>(out var SpriteRenderer))
                {
                    Undo.RecordObject(SpriteRenderer, "Load Pose");
                    SpriteRenderer.sortingOrder = layerPart.layerOrder;
                }
            }

        }
        catch { }
      
#if UNITY_EDITOR
        // 结束录制
       // AnimationMode.StopAnimationMode();
#endif 
        /*
        if (gameObject.TryGetComponent(out Animator animator))
        {
            float time = AnimationWindowUtilities.GetCurrentTime();
        }*/
    }
    string GetPartPath(Transform child,Transform root)
    {
        string path = child.name;
        while (child.parent != root)
        {
            path = $"{child.parent.name}/{path}";
            child = child.parent;
        }
        return path;
    }


    void GetClipBindings(AnimationClip animationClip)
    {
        EditorCurveBindingDic.Clear();
        EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(animationClip);
        for(int i = 0; i < bindings.Length; i++)
        {
            var binding = bindings[i];
            if(!EditorCurveBindingDic.TryGetValue(binding.path,out var dic))
            {
                dic = new Dictionary<string, EditorCurveBinding>();
                EditorCurveBindingDic.Add(binding.path, dic);
            }
            dic[binding.propertyName] = binding;
        }
    }

    Dictionary<string, Dictionary<string, EditorCurveBinding>> EditorCurveBindingDic = new Dictionary<string, Dictionary<string, EditorCurveBinding>>();
    private void RecordTransform(Vector3 pos,Vector3 angle,Vector3 scale, AnimationClip clip, float time, string path)
    {
       Type type= typeof(Transform);
       // 位置
        RecordVector3(clip, type,"m_LocalPosition", time, pos, path);
        // 旋转
        RecordVector3(clip, type,"m_LocalEulerAngles", time, angle, path);
        // 缩放
        RecordVector3(clip, type, "m_LocalScale", time, scale, path);
    }
   
    private void RecordVector3(AnimationClip clip, Type type, string propertyPath, float time, Vector3 value, string path)
    {
        // X轴
        SetKeyframe(clip, type, $"{propertyPath}.x", time, value.x,path);
        // Y轴
        SetKeyframe(clip, type, $"{propertyPath}.y", time, value.y,path);
        // Z轴
        SetKeyframe(clip, type, $"{propertyPath}.z", time, value.z, path);
    }
    private void SetKeyframe(AnimationClip clip,Type type,string propertyPath, float time, float value,string path)
    {
       if(!EditorCurveBindingDic.TryGetValue(path,out var dic))
        {
            dic = new Dictionary<string, EditorCurveBinding>();
            EditorCurveBindingDic.Add(path, dic);
        }
       if(!dic.TryGetValue(propertyPath,out var binding))
        {
            binding = new EditorCurveBinding
            {
                path = path,
                propertyName = propertyPath,
                type = type
            };
            dic.Add(propertyPath, binding);
        }

        Keyframe key = new Keyframe(time, value);
        key.inTangent = float.PositiveInfinity;
        key.outTangent = float.PositiveInfinity;
        key.inWeight = 0;
        var curve = AnimationUtility.GetEditorCurve(clip, binding);
        List<Keyframe> keys = new List<Keyframe>();
        if (curve == null)
        {
            curve = new AnimationCurve();
        }
        else
        {
            keys = curve.keys.ToList();
        }
       

        if (keys.Count==0)
        {
            keys.Add(key); 
        }
        else
        {
            bool isHave = false;
            for(int i=0;i < keys.Count; i++)
            {
                if (keys[i].time == time)
                {
                    isHave = true;
                    keys[i] = key;
                    break;
                }
            }
            if (!isHave)
            {
                keys.Add(key);
            }
        }
        curve.keys = keys.ToArray();

        // 保存曲线
        AnimationUtility.SetEditorCurve(clip, binding, curve);
    }
# endif

}

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteBonePoseController))]
public class SpriteBonePoseControllerEditor : Editor
{
    SpriteBonePoseController SpriteBonePoseController => target as SpriteBonePoseController;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("保存数据"))
        {
            SpriteBonePoseController.TransformToPoseData();
        }

        if (GUILayout.Button("读取数据"))
        {
            SpriteBonePoseController.PoseDataToTransform();
        }

        if (GUILayout.Button("保存动画"))
        {
            SpriteBonePoseController.RecordAnimation();
        }
    }
}
#endif
