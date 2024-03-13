
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SkyEnviromentMono : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer sun;
    [SerializeField]
    SpriteRenderer sky; 
    [SerializeField]
    private Light2D directionLight;
    [SerializeField]
    private Light2D globalLight;

    [SerializeField]
    SpriteRenderer bg,sea;
    [SerializeField]
    ParticleSystemRenderer cloud;

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
    Vector2 skyBgStartPos, skyBgEndPos;
    Vector2 mapSize;
    float4 bgOffset;
    float2 cameraOffset;
    async void DisplaySky(DisplaySky displaySky)
    {
        bgOffset =float4.zero;
        if (displaySky.display)
        {
            sky.enabled = true;
            sun.enabled = true;
            bg.enabled = true;
            sea.enabled = true;
            cloud.enabled = true; 
        }
        else
        {
            sky.enabled = false;
            sun.enabled = false;
            bg.enabled = false;
            sea.enabled = false;
            cloud.enabled=false;
        }

        var skyBackGroundData = await GameDataManager.instance.GetAsyncData<SkyBackGroundData>(displaySky.skyId);
        if (skyBackGroundData != null)
        {
            bg.sprite = skyBackGroundData.backGround;
            cameraOffset = skyBackGroundData.cameraOffset;
            bgOffset = skyBackGroundData.bgOffset;
        }
        else
        {
            bg.sprite = null;
            cameraOffset = 0;
            bgOffset = 0;
        }
        skyBgStartPos = displaySky.startPos;
        skyBgEndPos = displaySky.endPos;
        mapSize = skyBgEndPos - skyBgStartPos;
        SetBgPos(CameraManager.instance.mainCamera.transform.position);
    }

    public void SetBgPos(Vector2 cameraPos)
    {
        float2 offsetValue = (cameraPos - skyBgStartPos) / mapSize;
        offsetValue = math.clamp(offsetValue, 0, 1);

        Vector2 pos = new Vector2(math.lerp(bgOffset.x, bgOffset.z, offsetValue.x), math.lerp(bgOffset.y, bgOffset.w, offsetValue.y));
        bg.transform.localPosition = pos;

        float cameraOffsetY= math.lerp( cameraOffset.x, cameraOffset.y, offsetValue.y);
        CameraManager.instance.SetCameraOffset(new Vector2(0, cameraOffsetY));
    }

}