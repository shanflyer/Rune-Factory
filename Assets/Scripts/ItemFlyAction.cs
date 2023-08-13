using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemFlyAction : MonoBehaviour
{
    private Vector3 Pos2;

    private float flySpeed;

    private Item item;
    [HideInInspector]
    public bool IsFlyEnd,IsReward;
    
	// Use this for initialization
	void Start () {
		
	}

    public async void InitItemFlyData(Vector2 _pos2,float _speed,Item _item)
    {
        IsFlyEnd = false;
        IsReward = false;
        Pos2= _pos2;
        flySpeed = _speed;
        item = _item;
        ItemData itemData = await GameDataManager.instance.GetAsyncObjectData<ItemData>(item.dataId);
        GetComponentInChildren<SpriteRenderer>().sprite = itemData.iconSprite;
        StartCoroutine("Flying");
    }
    IEnumerator Flying()
    {
        Vector3 p0 = transform.position;
        Vector3 p2 = Pos2;
        float x = (p2.x - p0.x) / 2.0f;
        float y = (p2.y - p0.y) / 2.0f+Vector3.Distance(p2,p0)/2.0f;
        Vector3 p1 = new Vector3(x, y, 0) + p0;
        float timeValue = 0;
        while (true)
        {

            transform.position=(1-timeValue)*(1-timeValue)*p0+2*timeValue*(1-timeValue)*p1+
            timeValue * timeValue * p2;
            timeValue += 0.02f*flySpeed;
            if (timeValue >= 1)
            {
                StopCoroutine("Flying");
                IsFlyEnd = true;
                GameComponentData.gameData.BattleMapAction.CheckFlyItem();
                
                
            }
            yield return new WaitForSeconds(0.02f);
        }
    }

    public void StartResward()
    {
        StartCoroutine("Rewarding");
    }
    IEnumerator Rewarding()
    {
        BattleMapAction battleMapAction = GameComponentData.gameData.BattleMapAction;
        Vector3 p0 = transform.position;
        
        Vector3 p1=Vector3.zero;
        if (battleMapAction.charactorObj != null)
        {
            p1 = battleMapAction.charactorObj.transform.position;
        }
        else if(battleMapAction.TeamPlayer0!=null)
        {
            p1=battleMapAction.TeamPlayer0.transform.position;
        }
        else
        {
            p1=battleMapAction.TeamPlayer1.transform.position;
        }
        float timeValue = 0;
        float scalevalue = transform.localScale.x;
        while (true)
        {
            timeValue += 0.02f*flySpeed*2;
            transform.position = p0 + (p1 - p0) * timeValue;
            scalevalue *= (1 - timeValue);
            if (timeValue >= 1)
            {
                StopAllCoroutines();
                IsReward = true;
                GameComponentData.gameData.BattleMapAction.CheckRewardItem();
            }
            yield return new WaitForSeconds(0.02f);
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
