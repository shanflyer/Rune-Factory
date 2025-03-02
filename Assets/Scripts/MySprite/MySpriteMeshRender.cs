using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
public class MySpriteMeshRender : MonoBehaviour
{
   
    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshFilter meshFilter;

    public Sprite m_Sprite;
    public Material m_Material;

    private void OnEnable()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            CreateMesh();
        }
    }
    private void Awake()
    {
        
    }

    void CreateMesh()
    {
        var outData = MySpriteMeshManager.instance.GetSpriteMesh(m_Sprite, m_Material);
        meshFilter.sharedMesh = outData.mesh;
        meshRenderer.sharedMaterial = outData.material;
        nowSprite = m_Sprite;
        nowMaterial = m_Material;
    }
    Sprite nowSprite;
    Material nowMaterial;
    private void LateUpdate()
    {
        if (nowSprite != m_Sprite|| nowMaterial != m_Material)
        {
            CreateMesh(); 
        } 
    }
    public void TestMesh()
    {
        CreateMesh();
    }
     
}
#if UNITY_EDITOR
[CustomEditor(typeof(MySpriteMeshRender))]
public class MySpriteMeshRenderEditor : Editor
{
    public MySpriteMeshRender MySpriteMeshRender => target as MySpriteMeshRender;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
        {
            MySpriteMeshRender.TestMesh();
        }
        
    }
}
#endif