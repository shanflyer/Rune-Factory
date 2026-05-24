using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class MySpriteMeshRender : MonoBehaviour
{

    [SerializeField]
    private MeshRenderer meshRenderer;
    [SerializeField]
    private MeshFilter meshFilter;
    public Sprite m_Sprite;

    public bool flip = false;
    public SpriteDrawMode spriteDrawMode;
     public float size=1;
    public Vector2 Size = Vector2.one;
    public Color m_Color;
    public Material m_Material;
    public Material materialInstance => meshRenderer.material;


#if UNITY_EDITOR
    [SerializeField]
    private SpriteResourceRenference SpriteResourceRenference;

    public void SetSpriteRenference()
    {
        SpriteResourceRenference.SetSprite(this);
    }
#endif

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
        if (Application.isPlaying)
        {
            meshFilter.mesh.colors = colors;
        }
        else
        {
            meshFilter.sharedMesh.colors = colors;
        }

        nowColor = m_Color;
    }

    bool oldflip;
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
        if (oldflip != flip)
        {
            oldflip = flip;
            if (!flip)
            {
                Vector3 scale = Vector2.one * Size;
                scale.z = 1;
                transform.localScale = scale;
            }
            else
            {
                transform.localScale = new Vector3(-Size.x, Size.y, 1);
            }
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
        if (GUILayout.Button("Refrerence"))
        {
            MySpriteMeshRender.SetSpriteRenference();
        }
    }
}
#endif
