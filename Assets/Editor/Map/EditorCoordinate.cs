using System.Collections;
using TMPro;
using UnityEngine;

public class EditorCoordinate : MonoBehaviour
{
    [SerializeField]
    TextMeshPro textMesh;
    public void SetText(string text)
    {
        textMesh.text = text;
    }
     
}