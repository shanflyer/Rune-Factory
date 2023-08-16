using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OldName;

public class NPCAnimationAction : MonoBehaviour
{
    public Animator animator;
    public Animator animator1;
    public Vector2Int goldCoordinate;
    public Vector2Int endCoordinate;
    public float costTime;
    private float shopingTime;
    public Charactor charactor;
    [HideInInspector]
    public List<Cell> roadCells;
    [HideInInspector]
    public List<Cell> AnimalCells;
    public bool isAnimal;

    private List<Vector2Int> goldVector2Ints;
    private Vector2Int npcStartVector2Int;
    private Vector2Int npcMoveGold;
	// Use this for initialization
	void Start ()
	{
	    

	}

    public void InitNPCAnimationData(NPCAnimationAction npcAnimationAction)
    {
        animator = GetComponent<Animator>();
        goldCoordinate = npcAnimationAction.goldCoordinate;
        endCoordinate = npcAnimationAction.endCoordinate;
        costTime = npcAnimationAction.costTime;
        charactor = npcAnimationAction.charactor;
        roadCells = npcAnimationAction.roadCells;
        AnimalCells = npcAnimationAction.AnimalCells;
        isAnimal = npcAnimationAction.isAnimal;
        MoveRandom();
    }
    public void InitNPCAnimationData(Charactor _charactor, float _costTime,float _shopingTime,
        Vector2Int _endCoordinate,Vector2Int _goldCoordinate)
    {
        charactor = _charactor;
        costTime = _costTime;
        shopingTime = _shopingTime;
        endCoordinate = _endCoordinate;
        goldCoordinate = _goldCoordinate;
        StartMove();

    }
    public void InitAnimalAnimationData(Charactor _charactor, float _costTime,List<Cell> rangeCells)
    {
        charactor = _charactor;
        costTime = _costTime;
        isAnimal = true;
        AnimalCells = rangeCells;
        MoveRandom();

    }
    void CreatRoadCells(Vector2Int start,Vector2Int gold)
    {
        roadCells = AStarTest.RunAStar(start, gold, charactor);
    }
    public void SetDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.UP:
                animator.SetFloat("X",0);
                animator.SetFloat("Y",1);
                if (animator1 != null)
                {
                    animator1.SetFloat("X", 0);
                    animator1.SetFloat("Y", 1);
                }
                break;
            case Direction.DOWN:
                animator.SetFloat("X", 0);
                animator.SetFloat("Y", -1);
                if (animator1 != null)
                {
                    animator1.SetFloat("X", 0);
                    animator1.SetFloat("Y", -1);
                }
                break;
            case Direction.LEFT:
                animator.SetFloat("X", -1);
                animator.SetFloat("Y", 0);
                if (animator1 != null)
                {
                    animator1.SetFloat("X", -1);
                    animator1.SetFloat("Y", 0);
                }
                break;
            case Direction.RIGHT:
                animator.SetFloat("X", 1);
                animator.SetFloat("Y", 0);
                if (animator1 != null)
                {
                    animator1.SetFloat("X", 1);
                    animator1.SetFloat("Y", 0);
                }
                break;
        }
    }

    public void MoveRandom()
    {
        List<Cell> rangeCells= AnimalCells;
        int index = Random.Range(0, rangeCells.Count);
        Cell goldCell = rangeCells[index];
        goldCoordinate = goldCell.coordinate;
        CreatRoadCells(charactor.coordinate, goldCoordinate);
        StartCoroutine("AnimalMoving");
    }
    public void MoveTurn(Cell cell)
    {
        if (charactor.coordinate.x < cell.coordinate.x)
        {
            SetDirection(Direction.RIGHT);
        }
        if (charactor.coordinate.x > cell.coordinate.x)
        {
            SetDirection(Direction.LEFT);
        }
        if (charactor.coordinate.y < cell.coordinate.y)
        {
            SetDirection(Direction.UP);
        }
        if (charactor.coordinate.y > cell.coordinate.y)
        {
            SetDirection(Direction.DOWN);
        }
    }
    public void MoveTurn(Vector2 goldCoordinate)
    {
        if (charactor.coordinate.x < goldCoordinate.x)
        {
            SetDirection(Direction.RIGHT);
        }
        if (charactor.coordinate.x > goldCoordinate.x)
        {
            SetDirection(Direction.LEFT);
        }
        if (charactor.coordinate.y < goldCoordinate.y)
        {
            SetDirection(Direction.UP);
        }
        if (charactor.coordinate.y > goldCoordinate.y)
        {
            SetDirection(Direction.DOWN);
        }
    }
    public void Idle()
    {
        animator.SetBool("IsWalk", false);
        if (animator1 != null)
        {
            animator1.SetBool("IsWalk", false);
        }
    }
    public void Walk()
    {
        animator.SetBool("IsWalk",true);
        if (animator1 != null)
        {
            animator1.SetBool("IsWalk", true);
        }
    }
    
    public void StartMove()
    {
        CreatRoadCells(charactor.coordinate, goldCoordinate);
        StartCoroutine("Moving");
    }

    public void StartNPCMoving(List<Vector2Int> _goldVector2Ints,Vector2Int startVector2Int,Charactor _charactor)
    {
        charactor = _charactor;
        npcStartVector2Int = startVector2Int;
        goldVector2Ints = _goldVector2Ints;
        npcMoveGold = goldVector2Ints[Random.Range(0, goldVector2Ints.Count)];
        costTime = GameComponentData.gameData.peopleAction.costTime;
        StartCoroutine("NPCMoving");
    }
    public void InitPlayerData(Charactor _charactor,Vector2Int _endCoordinate)
    {
        charactor = _charactor;
        endCoordinate = _endCoordinate;
        costTime = GameComponentData.gameData.peopleAction.costTime;
        animator = charactor.npcAnimationAction.animator;
        CreatRoadCells(charactor.coordinate, endCoordinate);
        StartCoroutine("PlayerMoving");
    }

    public void FishIngMove(Vector2Int goldVector2Int)
    {
        GameComponentData.gameData.gameManager.playerMoveType=PlayerMoveType.Fishing;
        costTime = GameComponentData.gameData.peopleAction.costTime;
        endCoordinate = goldVector2Int;
        CreatRoadCells(charactor.coordinate, endCoordinate);
        StartCoroutine("PlayerMoving");
    }
    public void MapEndMove(Vector2Int _endCoordinate)
    {
        costTime = GameComponentData.gameData.peopleAction.costTime;
        endCoordinate = _endCoordinate;
        CreatRoadCells(charactor.coordinate, endCoordinate);
        StartCoroutine("PlayerMoving");
    }
    IEnumerator PlayerMoving()
    {
        float timeValue = 0;
        Vector3 P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
        Vector3 P0 = transform.position;
        MoveTurn(roadCells[roadCells.Count - 1]);
        Walk();
        while (true)
        {
            timeValue += Time.deltaTime / costTime;
            transform.position = P0 + (P1 - P0) * timeValue;
            if (timeValue >= 1)
            {
                timeValue = 0;
                transform.position = P1;
                charactor.coordinate = roadCells[roadCells.Count - 1].coordinate;
                var sprites = charactor.Obj.GetComponentsInChildren<SpriteRenderer>();
                foreach (var componentsInChild in sprites)
                {
                    componentsInChild.sortingOrder = GameComponentData.gameData.passDataManager.NowPassData.mapSizeY -
                                                     charactor.coordinate.y;
                }
                sprites[sprites.Length - 1].sortingOrder = 0;

                roadCells.RemoveAt(roadCells.Count - 1);
                if (roadCells.Count > 0)
                {
                    P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
                    P0 = transform.position;
                    MoveTurn(roadCells[roadCells.Count - 1]);
                }
                else if (charactor.coordinate == endCoordinate)
                {
                    Idle();
                    if (GameComponentData.gameData.gameManager.playerMoveType == PlayerMoveType.Start)
                    {

                        GameComponentData.gameData.NpcManager.MoveEndTalk();
                    }
                    else
                    {
                        AudioController.instance.PlayAudio(SE.Run);
                    }
                    GameComponentData.gameData.gameManager.MoveEndAction();
                    GameComponentData.gameData.gameManager.CheakPlayerMoveEnd();
                    StopAllCoroutines();
                    
                }
            }


            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator AnimalMoving()
    {
        float timeValue = 0;
        Vector3 P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
        Vector3 P0 = transform.position;
        MoveTurn(roadCells[roadCells.Count - 1]);
        Walk();
        while (true)
        {
            timeValue += Time.deltaTime / costTime;
            transform.position = P0 + (P1 - P0) * timeValue;
            if (timeValue >= 1)
            {
                timeValue = 0;
                transform.position = P1;

                if (roadCells.Count > 0)
                {
                    charactor.coordinate = roadCells[roadCells.Count - 1].coordinate;
                    charactor.cell = roadCells[roadCells.Count - 1];
                    roadCells.RemoveAt(roadCells.Count - 1);
                }
                
                if (roadCells.Count > 0)
                {
                    

                    P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
                    P0 = transform.position;
                    MoveTurn(roadCells[roadCells.Count - 1]);
                }
                else
                {
                    float waitTime = Random.Range(0.1f, 1.5f);
                    yield return new WaitForSeconds(waitTime);
                    StopAllCoroutines();
                    MoveRandom();

                }
            }


            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator Moving()
    {
        float timeValue=0;
        Vector3 P1= AStarTest.CoordinateToPos(roadCells[roadCells.Count-1].coordinate);
        Vector3 P0 = transform.position;
        MoveTurn(roadCells[roadCells.Count - 1]);
        Walk();
        while (true)
        {
            timeValue += Time.deltaTime / costTime;
            transform.position = P0 + (P1 - P0) * timeValue;
            if (timeValue >= 1)
            {
                timeValue = 0;
                transform.position = P1;
                charactor.coordinate = roadCells[roadCells.Count - 1].coordinate;
                roadCells.RemoveAt(roadCells.Count - 1);
                if (roadCells.Count > 0)
                {
                    P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
                    P0 = transform.position;
                    MoveTurn(roadCells[roadCells.Count - 1]);
                }
                else if(charactor.coordinate == endCoordinate)
                {
                    Idle();
                    StopAllCoroutines();
                    GameComponentData.gameData.peopleAction.DestoryCharactory(charactor);
                }
                else
                {
                    SetDirection(Direction.DOWN);
                    int deskCount = GameComponentData.gameData.shopGoldDeskAction.openCount;
                    float trueShopTime = shopingTime / ((deskCount - 1) * 0.125f + deskCount);
                    yield return new WaitForSeconds(shopingTime);
                    charactor.deskAction.SellItem();
                    CreatRoadCells(charactor.coordinate,endCoordinate);
                    P0 = transform.position;
                    if (roadCells == null || roadCells.Count == 0 || roadCells.Count == 1)
                    {
                        Destroy(gameObject);
                    }
                    P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
                }
            }
            
            
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator NPCMoving()
    {
        
        roadCells = AStarTest.RunAStar(npcStartVector2Int, npcMoveGold,charactor);

        Vector3 P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
        Vector3 P0 = transform.position;
        float timeValue = 0;
        MoveTurn(roadCells[roadCells.Count - 1]);
        Walk();
        while (true)
        {
            timeValue += Time.deltaTime / costTime;
            transform.position = P0 + (P1 - P0) * timeValue;
            if (timeValue >= 1)
            {
                
                transform.position = P1;
                charactor.coordinate = roadCells[roadCells.Count - 1].coordinate;
                roadCells.RemoveAt(roadCells.Count - 1);
                if (roadCells.Count > 0)
                {
                    timeValue = 0;
                    P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
                    P0 = transform.position;
                    MoveTurn(roadCells[roadCells.Count - 1]);
                }
                else
                {
                    Idle();
                    float waitTime = Random.Range(GameComponentData.gameData.NpcManager.waitTime.x,
                        GameComponentData.gameData.NpcManager.waitTime.y);
                    yield return new WaitForSeconds(waitTime);
                    npcMoveGold = goldVector2Ints[Random.Range(0, goldVector2Ints.Count)];
                    npcStartVector2Int = charactor.coordinate;
                    roadCells = AStarTest.RunAStar(npcStartVector2Int, npcMoveGold, charactor);
                    P1 = AStarTest.CoordinateToPos(roadCells[roadCells.Count - 1].coordinate);
                    P0 = transform.position;
                    timeValue = 0;
                    MoveTurn(roadCells[roadCells.Count - 1]);
                    Walk();
                }

            }

            yield return new WaitForFixedUpdate();
        }
       
    }
    // Update is called once per frame
    void Update () {
		
	}
}
