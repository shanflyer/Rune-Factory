using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "Data/精灵资源引用")]
public class SpriteResourceRenference : ScriptableObject
{
    public Sprite sprite;
    public Vector2 offset;
    public float scaleValue = 1;
    public void SetImageSprite(Image image, Vector2 zeroSize,Vector2 overrideOffset)
    {
        image.sprite = sprite;
        var transform = image.transform as RectTransform;
        transform.localPosition = overrideOffset;
        image.rectTransform.sizeDelta = GameCommon.SetImageSize(sprite, zeroSize) * scaleValue;
    }
    public void SetImageSprite(Image image,Vector2 zeroSize)
    {
        image.sprite = sprite;
        var transform = image.transform as RectTransform;
        image.rectTransform.sizeDelta = GameCommon.SetImageSize(sprite, zeroSize)*scaleValue;
        transform.localPosition = offset;
    }
    public void SetImageSprite(Image image)
    {
        image.sprite = sprite;
        var transform = image.transform as RectTransform;
        transform.localPosition = offset;
    }
    public void SetImageSpriteScale(Image image,float scale=1)
    {
        image.sprite = sprite;
        var transform = image.transform as RectTransform;
        image.SetNativeSize();
        transform.localScale = new Vector3(scaleValue* scale, scaleValue* scale, 1);
        transform.localPosition = offset;
    }
    public void SetSprite(SpriteRenderer spriteRenderer)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        spriteRenderer.size = sprite.rect.size*0.01f * scaleValue;
    }
    public void SetSprite(MySpriteMeshRender spriteRenderer)
    {
        spriteRenderer.m_Sprite = sprite;

        spriteRenderer.transform.localScale = new Vector3(scaleValue,scaleValue,scaleValue);
    }
}
