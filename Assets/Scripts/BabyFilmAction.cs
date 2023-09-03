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

    public void HaveChildAction()
    {
        foreach (Transform child in GameComponentData.gameData.NpcParent)
        {
            child.gameObject.SetActive(false);
        }
        GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(false);
        femeal.SetActive(true);
        meal.SetActive(true);
        if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.female)
        {
            GameObject femealObj = GameComponentData.gameData.gameManager.playerCharactor.Obj;
            NPCData mealData = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried == true).npcData;
            GameObject mealObj = GameComponent.models.Find(m => m.name == mealData.ObjName);

            femealName = GameComponentData.gameData.gameManager.gamePlayer.name;
            femealhead = GameComponentData.gameData.gameManager.gamePlayer.IconName;
            mealName = mealData.name;
            mealhead = mealData.headName;



            List<GameObject> s0 = new List<GameObject>();
            List<GameObject> s1 = new List<GameObject>();
            foreach (Transform child in femealObj.transform.GetChild(0))
            {
                s0.Add(child.gameObject);
            }
            foreach (Transform child in femeal.transform.GetChild(0))
            {
                s1.Add(child.gameObject);
            }
            for (int i = 0; i < s0.Count; i++)
            {
                s1[i].GetComponent<SpriteRenderer>().sprite = s0[i].GetComponent<SpriteRenderer>().sprite;
            }

            List<GameObject> s2 = new List<GameObject>();
            List<GameObject> s3 = new List<GameObject>();
            foreach (Transform child in mealObj.transform.GetChild(0))
            {
                s2.Add(child.gameObject);
            }
            foreach (Transform child in meal.transform.GetChild(0))
            {
                s3.Add(child.gameObject);
            }

            for (int i = 0; i < s2.Count; i++)
            {
                s3[i].GetComponent<SpriteRenderer>().sprite = s2[i].GetComponent<SpriteRenderer>().sprite;
            }
        }
        else
        {
            GameObject mealObj = GameComponentData.gameData.gameManager.playerCharactor.Obj;
            NPCData femealData = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried == true).npcData;
            GameObject femealObj = GameComponent.models.Find(m => m.name == femealData.ObjName);


            mealName = GameComponentData.gameData.gameManager.gamePlayer.name;
            mealhead = GameComponentData.gameData.gameManager.gamePlayer.IconName;
            femealName = femealData.name;
            femealhead = femealData.headName;


            List<GameObject> s0 = new List<GameObject>();
            List<GameObject> s1 = new List<GameObject>();
            foreach (Transform child in femealObj.transform.GetChild(0))
            {
                s0.Add(child.gameObject);
            }
            foreach (Transform child in femeal.transform.GetChild(0))
            {
                s1.Add(child.gameObject);
            }
            for (int i = 0; i < s0.Count; i++)
            {
                s1[i].GetComponent<SpriteRenderer>().sprite = s0[i].GetComponent<SpriteRenderer>().sprite;
            }

            List<GameObject> s2 = new List<GameObject>();
            List<GameObject> s3 = new List<GameObject>();
            foreach (Transform child in mealObj.transform.GetChild(0))
            {
                s2.Add(child.gameObject);
            }
            foreach (Transform child in meal.transform.GetChild(0))
            {
                s3.Add(child.gameObject);
            }

            for (int i = 0; i < s2.Count; i++)
            {
                s3[i].GetComponent<SpriteRenderer>().sprite = s2[i].GetComponent<SpriteRenderer>().sprite;
            }
        }
        femeal.GetComponentInChildren<Animator>().SetBool("IsWalk", false);
        filmObj.SetActive(true);
        oldWoman.SetActive(true);
        childObj0.SetActive(true);

        //GameComponentData.gameData.talkTextsManager.TalkAction("1515", null);
    }

    public void SetChildName()
    {
        SetNamePanel.SetActive(true);
    }
    public void ChangeChild()
    {
        StartCoroutine("ChangeChilding");
    }

    IEnumerator ChangeChilding()
    {
        childObj0.SetActive(false);
        oldWoman.GetComponentInChildren<NPCAnimationAction>().SetDirection(Direction.UP);
        yield return new WaitForSeconds(waitTime/2);
        childObj1.SetActive(true);
        yield return new WaitForSeconds(waitTime/2);
        //GameComponentData.gameData.talkTextsManager.TalkAction("1516", null);
    }
    public void PlayPregnancy()
    {
        foreach (Transform child in GameComponentData.gameData.NpcParent)
        {
            child.gameObject.SetActive(false);
        }
        GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(false);
        femeal.SetActive(true);
        meal.SetActive(true);
        if (GameComponentData.gameData.gameManager.gamePlayer.gender == Gender.female)
        {
            GameObject femealObj = GameComponentData.gameData.gameManager.playerCharactor.Obj;
            NPCData mealData=GameComponentData.gameData.NpcManager.Npcxs.Find(n=>n.isMarried==true).npcData;
            GameObject mealObj=GameComponent.models.Find(m=>m.name==mealData.ObjName);

            femealName = GameComponentData.gameData.gameManager.gamePlayer.name;
            femealhead = GameComponentData.gameData.gameManager.gamePlayer.IconName;
            mealName = mealData.name;
            mealhead = mealData.headName;



            List<GameObject> s0 = new List<GameObject>();
            List<GameObject> s1 = new List<GameObject>();
            foreach (Transform child in femealObj.transform.GetChild(0))
            {
                s0.Add(child.gameObject);
            }
            foreach (Transform child in femeal.transform.GetChild(0))
            {
                s1.Add(child.gameObject);
            }
            for (int i = 0; i < s0.Count; i++)
            {
                s1[i].GetComponent<SpriteRenderer>().sprite = s0[i].GetComponent<SpriteRenderer>().sprite;
            }

            List<GameObject> s2 = new List<GameObject>();
            List<GameObject> s3 = new List<GameObject>();
            foreach (Transform child in mealObj.transform.GetChild(0))
            {
                s2.Add(child.gameObject);
            }
            foreach (Transform child in meal.transform.GetChild(0))
            {
                s3.Add(child.gameObject);
            }

            for (int i = 0; i < s2.Count; i++)
            {
                s3[i].GetComponent<SpriteRenderer>().sprite = s2[i].GetComponent<SpriteRenderer>().sprite;
            }
        }
        else
        {
            GameObject mealObj = GameComponentData.gameData.gameManager.playerCharactor.Obj;
            NPCData femealData = GameComponentData.gameData.NpcManager.Npcxs.Find(n => n.isMarried == true).npcData;
            GameObject femealObj = GameComponent.models.Find(m => m.name == femealData.ObjName);


            mealName = GameComponentData.gameData.gameManager.gamePlayer.name;
            mealhead = GameComponentData.gameData.gameManager.gamePlayer.IconName;
            femealName = femealData.name;
            femealhead = femealData.headName;


            List<GameObject> s0 = new List<GameObject>();
            List<GameObject> s1 = new List<GameObject>();
            foreach (Transform child in femealObj.transform.GetChild(0))
            {
                s0.Add(child.gameObject);
            }
            foreach (Transform child in femeal.transform.GetChild(0))
            {
                s1.Add(child.gameObject);
            }
            for (int i = 0; i < s0.Count; i++)
            {
                s1[i].GetComponent<SpriteRenderer>().sprite = s0[i].GetComponent<SpriteRenderer>().sprite;
            }

            List<GameObject> s2 = new List<GameObject>();
            List<GameObject> s3 = new List<GameObject>();
            foreach (Transform child in mealObj.transform.GetChild(0))
            {
                s2.Add(child.gameObject);
            }
            foreach (Transform child in meal.transform.GetChild(0))
            {
                s3.Add(child.gameObject);
            }

            for (int i = 0; i < s2.Count; i++)
            {
                s3[i].GetComponent<SpriteRenderer>().sprite = s2[i].GetComponent<SpriteRenderer>().sprite;
            }
        }
        femeal.GetComponentInChildren<Animator>().SetBool("IsWalk",false);
        filmObj.SetActive(true);

        //GameComponentData.gameData.talkTextsManager.TalkAction("1509",null);
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
                GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(true);
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
                GameComponentData.gameData.gameManager.playerCharactor.Obj.SetActive(true);
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
                oldWoman.GetComponentInChildren<NPCAnimationAction>().SetDirection(Direction.LEFT);
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
