using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YanhuaAction : MonoBehaviour
{
    public GameObject yanPro;

    public Vector2 cdTime;

    public Vector2 PosX, PosY;
	// Use this for initialization
	void Start ()
	{
	    StartCoroutine("fire");
	}

    IEnumerator fire()
    {
        while (true)
        {
            float time = Random.Range(cdTime.x, cdTime.y);
            float x = Random.Range(PosX.x, PosX.y);
            float y = Random.Range(PosY.x, PosY.y);
            GameObject yanObj = Instantiate(yanPro, new Vector3(x, y, 0), Quaternion.identity);
            yanObj.transform.SetParent(transform);
           yield return new WaitForSeconds(time);

        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
