
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SkyEnviromentMono : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer sun;
    [SerializeField]
    SpriteRenderer sky;
    [SerializeField]
    Transform other;
    [SerializeField]
    private Light2D directionLight;
    [SerializeField]
    private Light2D globalLight;

    [SerializeField]
    SpriteRenderer bg;

    public Transform Sun
    {
        get
        {
            if(sun == null)
            {
                return null;
            }
            return sun.transform;
        }
    }
    public Light2D GlobalLight=>globalLight;
    public Light2D DirectionLight=>directionLight;
    private void Awake()
    {
        GameActionManager.instance.AddListener<DisplaySky>(DisplaySky);
    }
    void DisplaySky(DisplaySky displaySky)
    {
        if (displaySky.display)
        {
            sky.enabled = true;
            sun.enabled = true;
            other.gameObject.SetActive(true);
        }
        else
        {
            sky.enabled = false;
            sun.enabled = false;
            other.gameObject.SetActive(false);
        }
    }
}