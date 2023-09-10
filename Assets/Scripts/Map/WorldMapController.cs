using System.Collections;
using UnityEngine;

public class WorldMapController : MonoBehaviour
{
    public static WorldMapController instance;
    WorldMapManager worldMapManager;
    [SerializeField]
    string worldName;
    private void OnEnable()
    {
        instance = this;
        worldMapManager = WorldMapManager.instance;
        Init();
    }

    async void Init()
    {
        if (Camera.main == null)
        {
            var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                Instantiate(cameraPrefab);
            }
        }
    }
    // Use this for initialization
    void Start()
    {
        GameActionManager.instance.QueueAction(new ChangeWorld
        {
            worldName = worldName
        });
    }
#if UNITY_EDITOR
    private void Update()
    {
        if (GameController.instance == null)
        {
            SingletonType.instance.UpData();
        }
    }
#endif 
}