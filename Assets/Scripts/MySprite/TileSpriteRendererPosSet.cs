using UnityEngine;
[DisallowMultipleComponent]
public class TileSpriteRendererPosSet : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; 
    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            gameObject.TryGetComponent(out spriteRenderer); 
        }
    }
    Vector3 oldPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetMaterialProperty();
    }

    void SetMaterialProperty()
    {
        oldPos = transform.position;
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetVector("_ObjectWorldPos", transform.position);
        spriteRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    // Update is called once per frame
    void Update()
    {
        if (oldPos != transform.position)
        {
            SetMaterialProperty();
        }
    }
}
