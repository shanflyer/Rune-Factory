using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

public class DebugTest : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI textMeshProUGUI;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool isSrpB = GraphicsSettings.useScriptableRenderPipelineBatching;
        textMeshProUGUI.text = $"ISSRP:{isSrpB}";
    }
}
