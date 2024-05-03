using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[RequireComponent(typeof(PolygonCollider2D))]
public class PolygonOutMesh : MonoBehaviour
{
    public PolygonCollider2D polygonCollider;
    public string outMeshPath;
    public string meshName;
#if UNITY_EDITOR
    public void CreatMesh()
    {
        if (polygonCollider != null)
        {
            Mesh mesh = polygonCollider.CreateMesh(false,false);
            mesh.hideFlags = HideFlags.None;
            AssetDatabase.CreateAsset(mesh, $"{outMeshPath}/{meshName}.asset");
        }
    }
#endif 
}
#if UNITY_EDITOR
[CustomEditor(typeof(PolygonOutMesh))]
public class PologonOutMeshEditor : Editor
{
    public PolygonOutMesh polygonOutMesh
    {
        get
        {
            return target as PolygonOutMesh;
        }
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Éú³ÉMesh"))
        {
            polygonOutMesh.CreatMesh();
        }
    }
}
#endif

