using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
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
   
    public Color m_Color;
    public Material m_Material; 


    private void OnEnable()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            CreateMesh();
        }
        meshRenderer.enabled = true;
        if (meshRenderer.sharedMaterial == null || meshFilter.sharedMesh == null)
        {
            CreateMesh();
        }
    }
    private void OnDisable()
    {
        meshRenderer.enabled = false;
    }
    private void Awake()
    {
        
    }

    void CreateMesh()
    {
        var outData = MySpriteMeshManager.instance.GetSpriteMesh(m_Sprite, m_Material);
       
        this.mesh= meshFilter.sharedMesh = outData.mesh;
        meshRenderer.sharedMaterial = outData.material;
        nowSprite = m_Sprite;
        nowMaterial = m_Material;
    }

    Sprite nowSprite;
    Material nowMaterial;
    Mesh mesh;
    Color nowColor; 

    void SetMeshColor()
    {
        if (meshFilter.sharedMesh == null)
        {
            return;
        }
        if (m_Color == Color.white)
        {
            meshFilter.sharedMesh = mesh;
            nowColor = m_Color;
            return;
        }
        int count = meshFilter.sharedMesh.vertexCount;
        Color[] colors=new Color[count];
        for(int i = 0; i < count; i++)
        {
            colors[i] = m_Color;
        }
        
        meshFilter.mesh.colors = colors;
        nowColor = m_Color;
    }
    private void LateUpdate()
    {
        if (nowSprite != m_Sprite|| nowMaterial != m_Material)
        {
            CreateMesh(); 
        }
        if (m_Color != nowColor)
        {
            SetMeshColor();
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