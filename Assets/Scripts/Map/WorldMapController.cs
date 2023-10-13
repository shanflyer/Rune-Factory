using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldMapController : MonoBehaviour
{
    public static WorldMapController instance;
    WorldMapManager worldMapManager;
    [SerializeField]
    string worldName;
    [SerializeField]
    int characterId;
    [SerializeField]
    int mapInstance;
    [SerializeField]
    int2 coordinate;
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
            GameObjectCurveController.instance.SetUpDataComponent(this);
            worldMapManager.displayMap = mapInstance;
           
            var characterManager = CharacterManager.instance;
            var gameManager = GameManager.instance;
            var playerStoreManager= PlayerStoreManager.instance;
            
            var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                Instantiate(cameraPrefab);
            } 
            InputManager.instance.SwitchInputMap(false);
          
            GameActionManager.instance.QueueAction(new ChangeWorld
            {
                worldName = worldName,
                displayMap=mapInstance
            });
            GameActionManager.instance.QueueAction(new CreatCharacter
            {
                characterId = characterId,
                mapInstance = mapInstance,
                coordinateX = coordinate.x,
                coordinateY = coordinate.y,
                controller = true
            });
            GameActionManager.instance.QueueAction(new CreatDefaultNPC());
        }

        UIManager.instance.ShowGamePanel<PlayerTopPanel>();
    }
    // Use this for initialization
    void Start()
    {
       
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