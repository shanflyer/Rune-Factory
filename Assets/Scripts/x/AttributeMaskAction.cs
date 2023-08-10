using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeMaskAction : MonoBehaviour
{
    public List<GameObject> objs;

    private float value;
	// Use this for initialization
	void Start () {
		
	
	}

    public void SetSortLayer(int layerOrder)
    {
        GetComponent<SpriteRenderer>().sortingOrder = layerOrder;
        foreach (var o in objs)
        {
            o.GetComponent<SpriteRenderer>().sortingOrder = layerOrder;
        }
    }
    public void AfterAnimation()
    {
        GetComponent<Animator>().SetBool("hurt",false);
        
        foreach (GameObject child in transform.GetChild(1))
        {
            objs.Add(child);
        }
        foreach (var o in objs)
        {
            o.SetActive(false);
        }
        if (value < 0.8f && value >= 0.6f)
        {
            objs[0].SetActive(true);
        }
        else if (value < 0.6f && value >= 0.4f)
        {
            objs[1].SetActive(true);
        }
        else if (value < 0.4f && value >= 0.2f)
        {
            objs[2].SetActive(true);
        }
        else if (0<value&& value < 0.2f)
        {
            objs[3].SetActive(true);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void SetMaskDisplay(float _value)
    {
        value = _value;
        GetComponent<Animator>().SetBool("hurt", true);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
