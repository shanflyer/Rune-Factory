using UnityEngine;

public class ShowItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer itemRenderer;
    [SerializeField] private Animation animation;

    private int mapInstance;
    private RuntimeObj _runtimeObj;

    private void OnEnable()
    {
        GameActionManager.instance.AddListener<DisplayMap>(DisplayMap);
    }

    private void OnDisable()
    {
        if (!SingletonType.Cleared)
            GameActionManager.instance.RemoveListener<DisplayMap>(DisplayMap);
    }

    private void DisplayMap(DisplayMap displayMap)
    {
        if (displayMap.displayMap != mapInstance) Clear();
    }

    public void Show(Sprite sprite, int mapInstance, Vector3 position, RuntimeObj runtimeObj)
    {
        transform.position = position;
        itemRenderer.sprite = sprite;
        animation.Play();
        this.mapInstance = mapInstance;
        _runtimeObj = runtimeObj;
        if (animation.clip != null)
        {
            var length = animation.clip.length;
            GameTimerController.instance.DelayAction((int)(length * 1000), Clear);
        }
    }

    public void Clear()
    {
        if (_runtimeObj != null && _runtimeObj.obj.gameObject.activeSelf)
            GameRuntimeObjManager.instance.RecycleRuntimeObj(_runtimeObj);
    }
}