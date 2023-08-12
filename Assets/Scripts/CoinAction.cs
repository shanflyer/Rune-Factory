using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin
{
    public GameObject obj;
    public int value;
}
public class CoinAction : MonoBehaviour
{
    public Transform CoinParent,CoinCountParent;
    public Vector2 pos0, pos1;
    public Sprite CoinSprite0, CoinSprite1, CoinSprite2;

    public GameObject CoinPro,CoinCountPro;

    public List<Coin> Coins,FlyCoins;

    public int GroundMoney;

    public Vector3 DeskPos;
    public List<Vector3> endPos;
    // Use this for initialization
    void Start () {
		FlyCoins=new List<Coin>();
	}

    public void CreatGroundCoin()
    {
        foreach (Transform child in CoinParent)
        {
            Destroy(child.gameObject);
        }
        if (Coins != null)
        {
            Coins.Clear();
        }
        if (GroundMoney > 0)
        {
            CreatCoin(GroundMoney);
        }
        
    }
    public void CreatCoin(int value)
    {
        if (Coins == null)
        {
            Coins=new List<Coin>();
        }
        int count0 = value / 1000;
        int count1 = (value % 1000) / 100;
        int count2 = (value % 100)/10;
        if (count0 > 0)
        {
            for (int i = 0; i < count0; i++)
            {
                Coin coin = new Coin()
                {
                    value = 1000,
                };
                Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
                coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
                coin.obj.transform.SetParent(CoinParent);
                coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite0;
                Coins.Add(coin);
            }
           
        }
        if (count1 > 0)
        {
            for (int i = 0; i < count1; i++)
            {
                Coin coin = new Coin()
                {
                    value = 100,
                };
                Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
                coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
                coin.obj.transform.SetParent(CoinParent);
                coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite1;
                Coins.Add(coin);
            }

        }
        if (count2 > 0)
        {
            for (int i = 0; i < count2; i++)
            {
                Coin coin = new Coin()
                {
                    value = 10,
                };
                Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
                coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
                coin.obj.transform.SetParent(CoinParent);
                coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite2;
                Coins.Add(coin);
            }

        }
        int count3= (value % 10);
        if (count3 > 0)
        {
            Coin coin = new Coin()
            {
                value = count3,
            };
            Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
            coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
            coin.obj.transform.SetParent(CoinParent);
            coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite2;
            Coins.Add(coin);
        }
    }

    public void CreatCoin(int value, Vector3 _deskPos)
    {
        GroundMoney += value;
        DeskPos = _deskPos;
        if (endPos == null)
        {
            endPos = new List<Vector3>();
        }
        if (Coins == null)
        {
            Coins = new List<Coin>();
        }
        int count0 = value / 1000;
        int count1 = (value % 1000) / 100;
        int count2 = (value % 100)/10;
        if (count0 > 0)
        {
            for (int i = 0; i < count0; i++)
            {
                Coin coin = new Coin()
                {
                    value = 1000,
                };
                Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
                coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
                coin.obj.transform.SetParent(CoinParent);
                coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite0;
                Coins.Add(coin);
                FlyCoins.Add(coin);
            }

        }
        if (count1 > 0)
        {
            for (int i = 0; i < count1; i++)
            {
                Coin coin = new Coin()
                {
                    value = 100,
                };
                Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
                coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
                coin.obj.transform.SetParent(CoinParent);
                coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite1;
                Coins.Add(coin);
                FlyCoins.Add(coin);
            }

        }
        if (count2 > 0)
        {
            for (int i = 0; i < count2; i++)
            {
                Coin coin = new Coin()
                {
                    value = 10,
                };
                Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y), -2);
                coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
                coin.obj.transform.SetParent(CoinParent);
                coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite2;
                Coins.Add(coin);
                FlyCoins.Add(coin);
            }

        }
        int count3 = (value % 10);
        if (count3 > 0)
        {
            Coin coin = new Coin()
            {
                value = count3,
            };
            Vector3 pos = new Vector3(Random.Range(pos0.x, pos1.x), Random.Range(pos0.y, pos1.y),-2);
            coin.obj = Instantiate(CoinPro, pos, Quaternion.identity);
            coin.obj.transform.SetParent(CoinParent);
            
            coin.obj.GetComponentInChildren<SpriteRenderer>().sprite = CoinSprite2;
            Coins.Add(coin);
            FlyCoins.Add(coin);
        }

        foreach (var coin in FlyCoins)
        {
            endPos.Add(coin.obj.transform.position);
        }
        StartCoroutine("CoinFly");
    }

    public void ClickCoin(GameObject obj)
    {
        AudioController.instance.PlayAudio(SE.Coin);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(obj.transform.position);
        float x = Screen.width;
        float y = Screen.height;

        screenPos=new Vector3(screenPos.x/x*640,screenPos.y/y*1136,screenPos.z);
        

        screenPos=new Vector3(screenPos.x,screenPos.y,screenPos.z);
        
        //screenPos = new Vector3(screenPos.x - 320, screenPos.y - 568, screenPos.z);

        GameObject countObj = Instantiate(CoinCountPro, screenPos, Quaternion.identity);
        countObj.transform.SetParent(CoinCountParent,false);
        countObj.transform.localScale=Vector3.one;

        int index = Coins.FindIndex(c => c.obj == obj);
        countObj.GetComponentInChildren<Text>().text = Coins[index].value.ToString();
        GroundMoney -= Coins[index].value;
        if (FlyCoins.Contains(Coins[index]))
        {
            int index0 = FlyCoins.FindIndex(c => c == Coins[index]);
            endPos.RemoveAt(index0);
            FlyCoins.Remove(Coins[index]);
        }
        Coins.RemoveAt(index);
        
        Destroy(obj);
        GameComponentData.gameData.heritageAction.CheckHeritagesData();
    }

    public void LeaveShopMap()
    {
        if (Coins != null)
        {
            foreach (var coin in Coins)
            {
                Destroy(coin.obj);
            }
            
        }
        StopAllCoroutines();
        FlyCoins=new List<Coin>();
    }
    IEnumerator CoinFly()
    {
        
       
        float t = 0;
        while (true)
        {
            if (t >= 1)
            {
                t = 1;
                FlyCoins.Clear();
                endPos.Clear();
                StopCoroutine("CoinFly");
               
            }
            for (int i = 0; i < FlyCoins.Count; i++)
            {
                FlyCoins[i].obj.transform.position = DeskPos + (endPos[i] - DeskPos) * t;
            }
            t += 0.08f;
            yield return new WaitForSeconds(0.02f);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
