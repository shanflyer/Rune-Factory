using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PromptAction : MonoBehaviour
{
    public float displayTime;

    private List<string> informations;

    private bool isStart;
	// Use this for initialization
	void Start () {
		
	}

    public void AddInformation(string information)
    {
        if (informations == null)
        {
            informations=new List<string>();
        }
        informations.Add(information);
        if (!isStart)
        {
            StartCoroutine("Display");
        }
    }
  
    IEnumerator Display()
    {
        while (true)
        {
            if (informations.Count != 0)
            {
                isStart = true;
                string content = informations[0];
                GetComponentInChildren<Text>().text = content;
                informations.RemoveAt(0);
                
            }
            else
            {
                StopAllCoroutines();
                isStart = false;
                gameObject.SetActive(false);
            }
            yield return new WaitForSeconds(displayTime);
        }

        
        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
