using UnityEngine;

public class SnowHideMono : MonoBehaviour
{
    private void OnEnable()
    {
        GameTimeManager.instance.AddSnowHideMono(this);
    }

    private void OnDisable()
    {
        if (!SingletonType.Cleared) GameTimeManager.instance.RemoveSnowHideMono(this);
    }

    public void HideAction(bool hide)
    {
        transform.localScale = hide ? Vector3.zero : Vector3.one;
    }
}
