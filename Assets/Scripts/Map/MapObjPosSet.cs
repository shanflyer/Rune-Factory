using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[DisallowMultipleComponent]
public class MapObjPosSet : MonoBehaviour
{
    public bool staticObj = true;
   
    private Material mat;
    private void OnEnable()
    {
        if (mat == null)
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            mat = spriteRenderer.material;
        }
        Invoke("SetMatPos", 0.2f); 
    }
    
    void SetMatPos()
    {
        oldPos = transform.position;
        mat.SetVector("ObjPos", oldPos);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    Vector3 oldPos;
    private void LateUpdate()
    {
        if(!staticObj&&oldPos!=transform.position)
        {
            SetMatPos();
        }
    }
}
