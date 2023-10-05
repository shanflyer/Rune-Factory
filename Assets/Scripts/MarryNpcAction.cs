using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class MarryNpcAction : MonoBehaviour
{
    public Transform left, right,down;

    public Transform boy, gril;
	// Use this for initialization
	void Start () {
	    foreach (Transform transform1 in left)
	    {
	        transform1.GetComponent<Animator>().SetFloat("X",1);
	        transform1.GetComponent<Animator>().SetFloat("Y", 0);
        }
	    foreach (Transform transform1 in right)
	    {
	        transform1.GetComponent<Animator>().SetFloat("X", -1);
	        transform1.GetComponent<Animator>().SetFloat("Y", 0);
	    }
	    HideNpc();

	}

    void ChangeModel(GameObject x, string objName)
    {
        GameObject npcPro = GameComponent.models.Find(m => m.name == objName);
        GameObject obj = Instantiate(npcPro, x.transform.position, Quaternion.identity);
        List<GameObject> s0=new List<GameObject>();
        List<GameObject> s1 = new List<GameObject>();
        foreach (Transform child in obj.transform.GetChild(0))
        {
            s0.Add(child.gameObject);
        }
        foreach (Transform child in x.transform.GetChild(0))
        {
            s1.Add(child.gameObject);
        }

        for (int i = 0; i < s0.Count; i++)
        {
            s1[i].GetComponent<SpriteRenderer>().sprite = s0[i].GetComponent<SpriteRenderer>().sprite;
        }
        Destroy(obj);

        //obj.GetComponent<Animator>().enabled = false;

    }
    public void HideNpc()
    {
       

        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
