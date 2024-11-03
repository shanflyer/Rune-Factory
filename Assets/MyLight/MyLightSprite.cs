using UnityEngine;

public class MyLightSprite : MyLightBase
{
    
    SpriteRenderer spriteRenderer=>renderer as SpriteRenderer;
    public override void RefreshColor()
    {
        base.RefreshColor();
        spriteRenderer.color = lightColor;
    }
    public override void Test()
    {
        base.Test();
        RefreshColor();
    }
}
