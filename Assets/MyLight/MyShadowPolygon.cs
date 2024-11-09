using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
//[ExecuteAlways]
public class MyShadowPolygon : MonoBehaviour
{
    [SerializeField]
    PolygonCollider2D polygonCollider;
    [SerializeField]
    MeshFilter meshFilter;
    [SerializeField]
    MeshRenderer meshRenderer;
    [SerializeField]
    float scale = 1;
    [SerializeField]
    float shadowLength;
    [SerializeField]
    float shadow2Length;
    [SerializeField]
    float shadowOffset;
    [SerializeField]
    Color color;

    public PolygonCollider2D PolygonCollider => polygonCollider;
    private void OnEnable()
    {
        if (meshRenderer == null || meshRenderer.sharedMaterial == null)
        {
            polygonCollider = GetComponent<PolygonCollider2D>();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = Resources.Load<Material>("MyShadow");
        }
        //CreatMesh();
    }
    private void Awake()
    {
        
       
    }
    IEnumerator WaitToCreatMesh()
    {
        yield return new WaitForSeconds(0.1f);
        CreatMesh();
    }
    private void Start()
    {
        StartCoroutine(WaitToCreatMesh());
    }
    void InitMeshData(Vector2 uv1,Vector2 uv2,float scale, float offset, ref List<Vector3> verticeList,ref List<int> triangleList,ref List<Vector2> uv2s
        ,ref List<Color> colors,ref List<Vector2> uvs)
    {
        List<Vector3> verticeList1 = new List<Vector3>();
        verticeList1.AddRange(verticeList);
        List<int> triangleList1 = new List<int>();
        triangleList1.AddRange(triangleList);
        List<Vector2> uv2s1 = new List<Vector2>();
        uv2s1.AddRange(uv2s);
        List<Color> colors1 = new List<Color>();
        List<Vector2> uvs1 = new List<Vector2>();
        uvs1.AddRange(uvs);

        for (int i = 0; i < verticeList.Count; i++)
        {
            verticeList1.Add(verticeList[i]* scale+ verticeList[i].normalized*offset);
            if (uv2.y == 0)
            {
                uv2s1.Add(uv2s[i]);
            }
            else
            {
                uv2s1.Add(uv2);
            }
           
            uvs1.Add(uv1);
        }

        int tc = triangleList.Count / 3;
        for (int i = 0; i < tc; i++)
        {
            int index0 = triangleList[i * 3];
            int index1 = triangleList[i * 3 + 1];
            int index2 = triangleList[i * 3 + 2];

            int newIndex0 = index0 + verticeList.Count;
            int newIndex1 = index1 + verticeList.Count;
            int newIndex2 = index2 + verticeList.Count;

            triangleList1.Add(newIndex0);
            triangleList1.Add(newIndex1);
            triangleList1.Add(index0);

            triangleList1.Add(newIndex1);
            triangleList1.Add(index1);
            triangleList1.Add(index0);


            triangleList1.Add(newIndex1);
            triangleList1.Add(newIndex2);
            triangleList1.Add(index1);

            triangleList1.Add(newIndex2);
            triangleList1.Add(index2);
            triangleList1.Add(index1);

            triangleList1.Add(newIndex2);
            triangleList1.Add(newIndex0);
            triangleList1.Add(index2);

            triangleList1.Add(newIndex0);
            triangleList1.Add(index0);
            triangleList1.Add(index2);
        }

        
        for (int i = 0; i < verticeList1.Count; i++)
        { 
            colors1.Add(color);
        }

        verticeList = verticeList1;
        triangleList = triangleList1;
        uv2s = uv2s1;
        colors = colors1;
        uvs = uvs1;
    }

    public void CreatMesh()
    {
        try
        {
            var mesh = polygonCollider.CreateMesh(false, false);

            var newMesh = new Mesh();
            newMesh.Clear();
            Vector3 pos = transform.position;
            pos.z = 0;
            List<Vector3> verticeList = new List<Vector3>();
            List<int> triangleList = mesh.triangles.ToList();
            List<Vector2> uv2s = new List<Vector2>();
            List<Color> colors = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                verticeList.Add((mesh.vertices[i] - pos) * 1f);
                uv2s.Add(new Vector2(shadowOffset, 1));
                uvs.Add(Vector2.zero);
            }

            InitMeshData(Vector2.zero, new Vector2(shadowLength, 1),
                scale, 0, ref verticeList, ref triangleList, ref uv2s, ref colors, ref uvs);

            InitMeshData(Vector2.one, new Vector2(shadowLength, 0),
                1, shadow2Length, ref verticeList, ref triangleList, ref uv2s, ref colors, ref uvs);

            newMesh.vertices = verticeList.ToArray();
            newMesh.colors = colors.ToArray();
            newMesh.uv = uvs.ToArray();
            newMesh.uv2 = uv2s.ToArray();
            newMesh.triangles = triangleList.ToArray();
            newMesh.RecalculateBounds();
            var bounds = mesh.bounds;

            Vector3 minOffset = bounds.min - bounds.center;
            Vector3 maxOffset = bounds.max - bounds.center;
            Vector3 min = bounds.center + minOffset - new Vector3(shadowLength + shadow2Length, shadowLength + shadow2Length, 0);
            Vector3 max = bounds.center + maxOffset + new Vector3(shadowLength + shadow2Length, shadowLength + shadow2Length, 0);

            bounds = new Bounds(Vector3.zero, max - min);
            newMesh.bounds = bounds;
            meshFilter.mesh = newMesh;
        }
        catch
        {

        }
       
    }
    
}
#if UNITY_EDITOR
/*
[CustomEditor(typeof(MyShadowPolygon))]
public class MyShadowPolygonEditor : Editor
{
    public MyShadowPolygon shadowPolygon
    {
        get
        {
            return target as MyShadowPolygon;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("²âÊÔ"))
        {
            shadowPolygon.CreatMesh();
        }
    }
}*/
#endif