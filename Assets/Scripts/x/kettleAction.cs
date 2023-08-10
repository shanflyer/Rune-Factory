using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class kettleAction : MonoBehaviour
{
    public GameObject mask;

    public void InitKettle()
    {
        float value = GameComponentData.gameData.farmAction.waterValue;
        Vector3 maskScale = mask.transform.localScale;
        mask.transform.localScale=new Vector3(maskScale.x,0.75f*value,maskScale.z);
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
