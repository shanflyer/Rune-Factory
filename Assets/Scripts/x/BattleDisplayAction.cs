using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDisplayAction : MonoBehaviour
{
    public List<Color> Colors;
    public Color MaskColor;
    public GameObject skillStartPro;

    public SpriteRenderer MapMask;
    public float maskChangeTime;
    private float waitTime;
	// Use this for initialization
	void Start () {
		
	}

    public void InitSkill(Vector3 pos,AttributeType attributeType)
    {
        GameObject skillStartObj = Instantiate(skillStartPro,pos,Quaternion.identity);
        Color color = Colors[(int) attributeType];
        skillStartObj.GetComponentInChildren<SpriteRenderer>().color = color;
        waitTime = skillStartObj.GetComponent<DestoryAfterTime>().timeValue;
        StartCoroutine("StartSkill");
    }
    IEnumerator StartSkill()
    {
        float timeValue = 0;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            timeValue += Time.deltaTime;
            Color color = Color.Lerp(new Color(0, 0, 0, 0), MaskColor, timeValue / maskChangeTime);
            MapMask.color = color;
            if (timeValue >= waitTime)
            {
                GameComponentData.gameData.skillManager.StartWaitSKillAction();
                StopAllCoroutines();
            }
        }
        
    }

    public void HideMapMask()
    {
        MapMask.color=new Color(0,0,0,0);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
