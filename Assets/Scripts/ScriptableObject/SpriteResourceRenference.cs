using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Data/精灵资源引用")]
public class SpriteResourceRenference : ScriptableObject
{
    public Sprite sprite;
    public Vector2 offset;
    public float scaleValue = 1;

    public void SetImageSprite(Image image)
    {
        image.sprite = sprite;
        var transform = image.transform as RectTransform;
        transform.localPosition = offset;
    }

    public void SetSprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = sprite.rect.size * scaleValue;
    }
}