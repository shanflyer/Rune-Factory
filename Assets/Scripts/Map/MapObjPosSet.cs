using UnityEngine;

//[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class MapObjPosSet : MonoBehaviour
{
   
    private Material mat;
    private void OnEnable()
    {
       // if (mat == null)
        {
           // var spriteRenderer = GetComponent<Renderer>();
          //  mat = spriteRenderer.material;
        }
       
    }
    
    void SetMatPos()
    {
       // oldPos = transform.position;
      //  mat.SetVector("ObjPos", oldPos);
       // mat.SetInt("NativePos", 0);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    Vector3 oldPos;
    private void LateUpdate()
    {
      //  if(oldPos!=transform.position)
      //  {
          //  SetMatPos();
       // }
    }
}
