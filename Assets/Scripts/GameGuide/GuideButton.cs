public class GuideButton : GameButton
{
    protected override void OnEnable()
    {
        base.OnEnable();
        onClick.AddListener(ClickAction);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        onClick.RemoveListener(ClickAction);
    }
    void ClickAction()
    {
        GameGuideManager.instance.GuideButtonAction();
    }
}
