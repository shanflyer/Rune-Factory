using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(PolygonCollider2D))]
[ExecuteAlways]
public class PolyColliderToMesh : MonoBehaviour
{
    [SerializeField]
    PolygonCollider2D PolygonCollider2D;
    [SerializeField]
    string outPath = "";

    public void CreatMeshAsset()
    {
        Mesh mesh = PolygonCollider2D.CreateMesh(false, false);
        AssetDatabase.CreateAsset(mesh, $"{outPath}/{PolygonCollider2D.name}.asset");
    }
    private void OnEnable()
    {
        PolygonCollider2D = GetComponent<PolygonCollider2D>();
    }
    private void Awake()
    {
       
    }
}
public class PolyColliderToMeshEditor : Editor
{
    public PolyColliderToMesh polyColliderToMesh
    {
        get
        {
            return target as PolyColliderToMesh;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("创建Mesh"))
        {
            polyColliderToMesh.CreatMeshAsset();
        }
    }
}