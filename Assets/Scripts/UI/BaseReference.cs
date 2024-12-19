using System.Collections;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class BaseReference : MonoBehaviour
{
    public static LayerMask UILayer;
    public static LayerMask HideLayer;

    [SerializeField]
    public Canvas canvas;
    [SerializeField]
    public GraphicRaycaster raycaster;
    public bool show; 
    public virtual bool changeInputModel { get=>true; }
    public virtual void SetPanelUISerializeObj()
    {
        gameObject.TryGetComponent(out canvas);
        gameObject.TryGetComponent(out raycaster);
    }
    public virtual void Show(int layer = -1) { show = true; }
    public virtual void Close() { show = false; } 
    public virtual async Task InitData(string dataKey) { }
   
}