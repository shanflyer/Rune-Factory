using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class TestRenderGroup : MonoBehaviour
{
    [SerializeField]
   Renderer[] renderers;
   // Sprite sprite;
    

    private void OnEnable()
    { 
        //sprite.GetSecondaryTextures()
        for(int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }
            CameraManager.instance.AddTestRender(renderers[i]);
        }
    }
    private void OnDisable()
    {
        
        if (!SingletonType.Cleared)
        {
            
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                {
                    continue;
                }
                CameraManager.instance.RemoveTestRender(renderers[i]);
            }

        }
    }
    public void GetRenders()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(TestRenderGroup))]
public class TestRenderGroupEditor :Editor
{
    public TestRenderGroup TestRenderGroup
    {
        get => target as TestRenderGroup;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("获取Renderers"))
        {
            TestRenderGroup.GetRenders();
            EditorUtility.SetDirty(TestRenderGroup);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif