using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class BabyFilmAction : MonoBehaviour
{
    public float speed, waitTime;

    public GameObject femeal, meal, oldWoman,childObj0, childObj1;
    public GameObject GuoduObj;
    public GameObject filmObj;
    public GameObject SetNamePanel;
    private string femealName, mealName;

    private string femealhead, mealhead;
    // Use this for initialization
    void Start () {
		
	}

  

    public void SetChildName()
    {
        SetNamePanel.SetActive(true);
    }
    public void ChangeChild()
    {
        StartCoroutine("ChangeChilding");
    }

  
  

    public void HaveChildEnd()
    {
        GuoduObj.SetActive(true);

        StartCoroutine("HaveChildEnding");
    }
    IEnumerator HaveChildEnding()
    {
        Image image = GuoduObj.GetComponentInChildren<Image>();
        float timeValue = 0;
        if (speed < 0)
        {
            speed = -speed;
        }
       
        while (true)
        {
            image.color = new Color(0, 0, 0, timeValue);
            if (timeValue >= 1)
            {
                timeValue = 1;
                yield return new WaitForSeconds(waitTime);
                oldWoman.SetActive(false);
                meal.SetActive(false);
                femeal.SetActive(false);
                childObj1.SetActive(false);
                DataSaveAndLoadTest.gameSaveData.marryData.SaveHaveChildrenTimeTime();
                GameComponentData.gameData.mapParent.GetComponentInChildren<GameBoxClickAction>().CheckChild();
                foreach (Transform child in GameComponentData.gameData.NpcParent)
                {
                    child.gameObject.SetActive(true);
                }
               // GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(true);
                filmObj.SetActive(false);
                speed = -speed;
            }
            timeValue += speed;
            if (timeValue <= 0)
            {

                filmObj.SetActive(false);

                yield return new WaitForSeconds(waitTime / 3);

               
                StopAllCoroutines();
                GuoduObj.SetActive(false);

            }
            yield return new WaitForSeconds(0.01f);
        }
    }
    public void Gudu()
    {
        GuoduObj.SetActive(true);
        StartCoroutine("Guduing");
    }

    public void End()
    {
        GuoduObj.SetActive(true);
        StartCoroutine("Ending");
    }
    IEnumerator Ending()
    {
        Image image = GuoduObj.GetComponentInChildren<Image>();
        float timeValue = 0;
        if (speed < 0)
        {
            speed = -speed;
        }
        while (true)
        {
            image.color = new Color(0, 0, 0, timeValue);
            if (timeValue >= 1)
            {
                timeValue = 1;
                yield return new WaitForSeconds(waitTime);
                oldWoman.SetActive(false);
                meal.SetActive(false);
                femeal.SetActive(false);
                foreach (Transform child in GameComponentData.gameData.NpcParent)
                {
                    child.gameObject.SetActive(true);
                }
               // GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(true);
                filmObj.SetActive(false);
                speed = -speed;
            }
            timeValue += speed;
            if (timeValue <= 0)
            {
                
                filmObj.SetActive(false);

                yield return new WaitForSeconds(waitTime / 3);

                DataSaveAndLoadTest.gameSaveData.marryData.SavePregnancyTime();

                StopAllCoroutines();
                GuoduObj.SetActive(false);
                
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
    IEnumerator Guduing()
    {
        Image image = GuoduObj.GetComponentInChildren<Image>();
        float timeValue = 0;
        if (speed < 0)
        {
            speed = -speed;
        }
        while (true)
        {
            image.color=new Color(0,0,0,timeValue);
            if (timeValue >= 1)
            {
                timeValue = 1;
                yield return new WaitForSeconds(waitTime);
                oldWoman.SetActive(true);
                //oldWoman.GetComponentInChildren<NPCAnimationAction>().SetDirection(Direction.LEFT);
                speed = -speed;
            }
            timeValue += speed;
            if (timeValue <= 0)
            {
               
                filmObj.SetActive(true);

                yield return new WaitForSeconds(waitTime/3);
                StopAllCoroutines();
                GuoduObj.SetActive(false);
                //GameComponentData.gameData.talkTextsManager.TalkAction("1511",null);
                
            }
            yield return new WaitForSeconds(0.01f);
        }
    }
    // Update is called once per frame
    void Update () {
		
	}
}
