using System.Collections;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class BaseReference : MonoBehaviour
{
    public static LayerMask UILayer;
    public static LayerMask HideLayer;
    public virtual bool pluralUI { get; }
    public virtual bool changeInputModel { get=>true; }
    public virtual void SetPanelUISerializeObj()
    { 
    }
    public virtual void Show(int layer = -1) { }
    public virtual void Close() { } 
    public virtual async Task InitData(string dataKey) { }
}