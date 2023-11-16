using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorldMapController : MonoBehaviour
{
    public static WorldMapController instance;
    WorldMapManager worldMapManager;
    [SerializeField]
    GameObject eventSystemObj;
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
        if (eventSystemObj&&UnityEngine.SceneManagement.SceneManager.sceneCount > 1)
        {
            eventSystemObj.SetActive(false);
        }
        Init();
       
    }

    public async void Init()
    {
        if (Camera.main == null)
        {
            var cameraPrefab = await GameSourceManager.instance.GetPrefab(DataPath.cameraPrefabPath);
            if (cameraPrefab != null)
            {
                Instantiate(cameraPrefab);
            } 
        }
       
        GameObjectCurveController.instance.SetUpDataComponent(this);
        WorldMapObjManager.instance.displayMap = mapInstance;

        var teamManager = TeamManager.instance;
        var npcManager = NPCManager.instance;
        var gameEventManager = GameEventManager.instance;
        var tempCharacterManager = TempCharacterManager.instance;
        var characterManager = CharacterManager.instance;
        var gameManager = GameManager.instance;
        var playerStoreManager = PlayerStoreManager.instance;
        var talkManager = TalkManager.instance;
        var friendManager = FriendManager.instance;
        var shopManager = ShopManager.instance;
        var farmManager = FarmManager.instance;
        var tempMapItemController = TempMapItemController.instance;
         
        GameActionManager.instance.QueueAction(new ChangeWorld
        {
            worldName = worldName,
            displayMap = mapInstance
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

        await UIManager.instance.ShowGamePanel<MainPanel>();
        InputManager.instance.SwitchInputMap(false);
        GameActionManager.instance.QueueAction(new InitInputAction());
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