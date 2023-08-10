using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InformationManager : MonoBehaviour
{
    [HideInInspector] public List<string> informations;
    public float minTime, MaxTime;
    public Text infomationText;
    public GameObject informationPanel;
    public GameObject informationPro;
    public Transform informationParent;
    private List<string> displayInformations;
    private List<GameObject> informationObjs;
    private bool isDisplay;
	// Use this for initialization
	void Start () {
		
	}

    public void InitData()
    {
        informations = new List<string>();
        displayInformations = new List<string>();
        informationObjs = new List<GameObject>();
        isDisplay = true;
    }
    public void DisplayInformationPanel()
    {
        AudioManager.PlaySE(PlayType.ONCE,"Click");
        informationPanel.SetActive(true);
        foreach (var information in informations)
        {
            GameObject informationObj = Instantiate(informationPro);
            informationObj.GetComponent<Text>().text = information;
            informationObj.transform.SetParent(informationParent);
            informationObj.transform.localScale=Vector3.one;
            informationObjs.Add(informationObj);
        }
        isDisplay = false;
    }

    public void ReturnFormInformationPanel()
    {
        foreach (Transform transform1 in informationParent)
        {
            Destroy(transform1.gameObject);
        }
        informationPanel.SetActive(false);
        isDisplay = true;
        if (displayInformations.Count > 0)
        {
            StartCoroutine("DisplayInformation");
        }
    }
    public void AddInformation(string information)
    {
        informations.Insert(0,information);
        if (informations.Count > 200)
        {
            informations.RemoveAt(200);
        }
        if (displayInformations == null)
        {
            displayInformations=new List<string>();
        }
        if (displayInformations.Count == 0 && isDisplay)
        {
            displayInformations.Add(information);
            StartCoroutine("DisplayInformation");
        }
        else
        {
            displayInformations.Add(information);
        }
        if (!isDisplay)
        {
            GameObject informationObj = Instantiate(informationPro);
            informationObj.GetComponent<Text>().text = information;
            informationObj.transform.SetParent(informationParent);
            informationObj.transform.SetSiblingIndex(0);
            informationObjs.Add(informationObj);
            if (informationObjs.Count > 200)
            {
                Destroy(informationObjs[200]);
                informationObjs.RemoveAt(200);
            }
        }
    }
    IEnumerator DisplayInformation()
    {
        while (true)
        {
            float waitTime = minTime;
            if (displayInformations.Count > 0)
            {
                
                if (displayInformations.Count <= 1)
                {
                    waitTime = MaxTime;
                }

                infomationText.text = displayInformations[0];
                
            }
            else
            {
                StopAllCoroutines();
            }
            
            yield return new WaitForSeconds(waitTime);
            if (displayInformations.Count > 0)
            {
                displayInformations.RemoveAt(0);
            }
            if (displayInformations.Count == 0)
            {
                infomationText.text = "";
            }
        }
        
    }
	// Update is called once per frame
	void Update () {
		
	}
}
