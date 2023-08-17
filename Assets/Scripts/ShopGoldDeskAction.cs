using OldName;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class ShopGoldDeskAction : MonoBehaviour
{
    public int zeroCost,plusCost;
    public GameObject deskPro;
    public Transform deskParent;
    public GameObject wareHouse;
    public GameObject DeskItemPanel;
    public List<Vector2Int> deskCoordinates;
    public List<Desk> GoodDeskes;
    private List<Vector2Int> OpenDeskCoordinates;
    public int openCount;
    public List<Vector2Int> StartCoordinates;
    public List<Vector2Int> EndCoordinatesInts;
    public float shopingTime;
    public Vector2 waitTimeRange;
    public GameObject NullDeskPro;
    private GameObject NullDesk;
    private float waitTime;
    private bool isBackSell;
    public int saleValue;
   


    private int countOpen=0;
	// Use this for initialization
	void Start ()
	{
        
	}
    public void LoadData(DeskData deskData)
    {
        openCount = deskData.deskCount;
        if (openCount <= 0)
        {
            openCount = 1;
        }
        CreatDesk();
        for (int i = 0; i < openCount; i++)
        {
            if (deskData.deskItemIds.Count==0||deskData.deskItemIds[i] == 0)
            {
                GoodDeskes[i].item = default(Item);
                GoodDeskes[i].deskAction.InitDeskData(default(Item));
            }
            else
            {
                Item item = ItemManager.instance.CreatItem(deskData.deskItemIds[i], deskData.deskItenCounts[i]);
                GoodDeskes[i].item = item;
                GoodDeskes[i].deskAction.InitDeskData(item);
            }
           
        }

        ShopingInBack();

    }
    public void CreatDesk()
    {
        if (GoodDeskes == null)
        {
            GoodDeskes=new List<Desk>();
            OpenDeskCoordinates = new List<Vector2Int>();
            for (int i = 0; i < openCount; i++)
            {
                OpenDeskCoordinates.Add(deskCoordinates[i]);
            }
           // ProfessionData deskProfessionData = CharactorDataAction.professionDatas[1];
           // InfluenceData deskInfluenceData = InfluenceAction.influenceDates0.Find(i => i.name == Influence.NPC0);
            for (int i = 0; i < OpenDeskCoordinates.Count; i++)
            {
                Vector2Int deskCoordinate = OpenDeskCoordinates[i];
                Vector3 pos = AStarTest.CoordinateToPos(deskCoordinate);
                GameObject deskObj = Instantiate(deskPro, pos, Quaternion.identity);
                DeskAction deskAction = deskObj.GetComponent<DeskAction>();
                deskAction.WarehouseObj = wareHouse;
                deskAction.DeskItemInformationObj = DeskItemPanel;
                deskObj.transform.SetParent(deskParent);
                Desk desk = new Desk(i, "货柜", deskObj, 1000, deskCoordinate, deskObj.GetComponent<DeskAction>());
                GoodDeskes.Add(desk);
            }
            if (openCount < deskCoordinates.Count)
            {
                Vector3 pos = AStarTest.CoordinateToPos(deskCoordinates[openCount]);
                NullDesk = Instantiate(NullDeskPro, pos, Quaternion.identity);
                NullDesk.transform.SetParent(deskParent);
            }
        }
        else
        {
            foreach (var goodDeske in GoodDeskes)
            {
                goodDeske.Obj.GetComponentInChildren<BoxCollider2D>().enabled = true;
                goodDeske.Obj.transform.Find("Canvas").gameObject.SetActive(true);
                var x = goodDeske.Obj.GetComponentsInChildren<SpriteRenderer>();
                foreach (var spriteRenderer in x)
                {
                    spriteRenderer.enabled = true;
                }
                if (goodDeske.deskAction.item.instanceId==0)
                {
                    goodDeske.deskAction.ItemSpriteRenderer.enabled = false;
                    goodDeske.deskAction.ItemCountText.enabled = false;
                }
                else
                {
                    goodDeske.deskAction.ItemSpriteRenderer.enabled = true;
                    goodDeske.deskAction.ItemCountText.enabled = true;
                }

            }
            if (NullDesk != null)
            {
                NullDesk.SetActive(true);
            }
           
        }
        
       
        
    }

   

  

 
    public void ClickAddDesk()
    {
        int costValue = zeroCost + openCount * plusCost;
        AudioController.instance.PlayAudio(SE.click);
        GameComponentData.gameData.gameManager.InitCostData("新柜台",costValue,"增加一个新柜台？",CostType.增加柜台,
            ShopMoneyType.金币);
    }

 

    public void AddNewDesk()
    {
        if (openCount < deskCoordinates.Count)
        {
            openCount++;
            OpenDeskCoordinates.Add(deskCoordinates[openCount-1]);
            Vector2Int deskCoordinate = OpenDeskCoordinates[openCount-1];
            Vector3 deskpos = AStarTest.CoordinateToPos(deskCoordinate);
            GameObject deskObj = Instantiate(deskPro, deskpos, Quaternion.identity);
            DeskAction deskAction = deskObj.GetComponent<DeskAction>();
            deskAction.WarehouseObj = wareHouse;
            deskAction.DeskItemInformationObj = DeskItemPanel;
            deskObj.transform.SetParent(deskParent);
            Desk desk = new Desk(openCount - 1, "货柜", deskObj, 1000, deskCoordinate, deskObj.GetComponent<DeskAction>());
            GoodDeskes.Add(desk);

           
            Destroy(NullDesk);
            if (openCount < deskCoordinates.Count)
            {
                Vector3 pos = AStarTest.CoordinateToPos(deskCoordinates[openCount]);
                NullDesk = Instantiate(NullDeskPro, pos, Quaternion.identity);
                NullDesk.transform.SetParent(deskParent);
            }
        }
        
    }

    public void SellInBack(int totalMin)
    {
        if (GoodDeskes != null)
        {
            var desks = GoodDeskes.FindAll(g => g.deskAction.item.count>0);
          
            List<float> RandomValue=new List<float>();
         
            foreach (var desk in desks)
            {
                Item item = desk.deskAction.item; 
                int ramdomValue = 8000; 
                RandomValue.Add((10000-ramdomValue)/10000.0f);
            }

            float unSellRandom = 1;
            foreach (var f in RandomValue)
            {
                unSellRandom *= f;
            }
            


            if (desks.Count > 0)
            {

                int totalCount = (int) (totalMin * ((openCount - 1) * 0.3f + 1));
                float sellCount = totalCount * (1 - unSellRandom);
                int itemCount = desks.Count;
                int x = Mathf.CeilToInt(sellCount / itemCount);
                
                List<int> sellCounts=new List<int>();
                foreach (var desk in desks)
                {
                    if (x > 0)
                    {
                        Item item = desk.deskAction.item;

                        if (item.count < x)
                        {
                            sellCounts.Add(item.count);
                            sellCount -= item.count;
                            desk.deskAction.SellItem(item.count);
                        }
                        else
                        {
                            sellCounts.Add(x);
                            sellCount -= x;
                            desk.deskAction.SellItem(x);
                        }
                        itemCount--;
                        if (itemCount > 0)
                        {
                            x = Mathf.CeilToInt(sellCount / itemCount);
                        }
                    }
                    
                }

            }
        }   
    }


    public void ShopingInBack()
    {
        if (!isBackSell)
        {
            if (GoodDeskes != null)
            {
                foreach (var desk in GoodDeskes)
                {
                    desk.Obj.GetComponentInChildren<BoxCollider2D>().enabled = false;
                    desk.Obj.transform.Find("Canvas").gameObject.SetActive(false);
                    var x = desk.Obj.GetComponentsInChildren<SpriteRenderer>();
                    foreach (var spriteRenderer in x)
                    {
                        spriteRenderer.enabled = false;
                    }
                    desk.deskAction.ItemCountText.enabled = false;
                }
                if (NullDesk != null)
                {
                    NullDesk.SetActive(false);
                }
                isBackSell = true;
                StopCoroutine("CreatNPC");
                StartCoroutine("BackGroundSell");
                countOpen++;
                GameComponentData.gameData.peopleAction.CleraAllNPC();
            }

            GameComponentData.gameData.coinAction.LeaveShopMap();
        }
        
        
    }
    public void StartNpcShoping()
    {
        isBackSell = false;
        GameComponentData.gameData.coinAction.CreatGroundCoin();
        StopCoroutine("BackGroundSell");
        StartCoroutine("CreatNPC");

    }
    IEnumerator CreatNPC()
    {
        float timeValue = 0;
        float trueWaitTime = 0;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            timeValue += Time.deltaTime;
            if (timeValue >= trueWaitTime)
            {
                waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y);
                int startIndex = Random.Range(0, StartCoordinates.Count - 1);
                Vector2Int startCoordinate = StartCoordinates[startIndex];
                int endIndex = Random.Range(0, EndCoordinatesInts.Count - 1);
                Vector2Int endCoordinate = EndCoordinatesInts[endIndex];
                Vector2Int goldCoordinate = endCoordinate;
                float waitValue = (1.0f - saleValue / 100.0f) / 0.1f;
                trueWaitTime = waitTime * Mathf.Pow(0.8f, waitValue)/((openCount-1)*0.2f+1);
                var goldDesks = GoodDeskes.FindAll(d => d.deskAction.item.dataId!=0);
                int goldIndex = 0;
                if (goldDesks.Count > 0)
                {

                    goldIndex = Random.Range(0, goldDesks.Count);
                    goldCoordinate = new Vector2Int(goldDesks[goldIndex].coordinate.x, goldDesks[goldIndex].coordinate.y + 1);
                }
                if (goldDesks.Count > 0)
                {
                    if (goldDesks[goldIndex].item.dataId != 0 && Random.Range(0,100)>50)
                    {
                        GameComponentData.gameData.peopleAction.CreatNPC(startCoordinate, endCoordinate, goldCoordinate, shopingTime, goldDesks[goldIndex].deskAction);
                    }
                    else
                    {
                        GameComponentData.gameData.peopleAction.CreatNPC(startCoordinate, endCoordinate, endCoordinate, 0, null);
                    }

                }
                else
                {
                    GameComponentData.gameData.peopleAction.CreatNPC(startCoordinate, endCoordinate, goldCoordinate, 0, null);
                }

                timeValue = 0;
            }
            

        }
    }

    IEnumerator BackGroundSell()
    {
        float timeValue = 0;
        float trueWaitTime = 0;
        while (true)
        {
            
            yield return new WaitForFixedUpdate();
            timeValue += Time.deltaTime;
            if (timeValue >= trueWaitTime)
            {
                float waitTime = Random.Range(waitTimeRange.x, waitTimeRange.y) * 1.2f;
                float waitValue = (1.0f - saleValue) / 0.1f;
                trueWaitTime = waitTime * Mathf.Pow(0.8f, waitValue) / ((openCount - 1) * 0.3f + 1);
                var goldDesks = GoodDeskes.FindAll(d => d.deskAction.item.dataId != 0);
                int goldIndex = 0;
                if (goldDesks.Count > 0)
                {
                    goldIndex = Random.Range(0, goldDesks.Count);

                }
                if (goldDesks.Count > 0)
                {
                    if (goldDesks[goldIndex].item.dataId != 0 && Random.Range(0,100)>60)
                    {
                        Desk desk = goldDesks[goldIndex];
                        desk.deskAction.SellItem();
                    }

                }
                else
                {
                    StopCoroutine("BackGroundSell");
                }

            }
           
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
