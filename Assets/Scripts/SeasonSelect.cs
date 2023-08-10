using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeasonSelect : MonoBehaviour
{
    public Transform LoverTransform;
    public List<GameObject> SprinGameObjects;

    public List<GameObject> AutumnGameObjects;

    public List<GameObject> WinterGameObjects;
	// Use this for initialization
	void Start () {
		
	}

    public void ClickWater()
    {
        GameComponentData.gameData.farmAction.InitWaterValue();
    }
    public void SeasonSetData(Season _season)
    {
        switch (_season)
        {
            case Season.春:
                foreach (var sprinGameObject in SprinGameObjects)
                {
                    sprinGameObject.SetActive(true);
                }
                foreach (var autumnGameObject in AutumnGameObjects)
                {
                    autumnGameObject.SetActive(false);
                }
                foreach (var winterGameObject in WinterGameObjects)
                {
                    winterGameObject.SetActive(false);
                }
                break;
            case Season.夏:
                foreach (var sprinGameObject in SprinGameObjects)
                {
                    sprinGameObject.SetActive(true);
                }
                foreach (var autumnGameObject in AutumnGameObjects)
                {
                    autumnGameObject.SetActive(false);
                }
                foreach (var winterGameObject in WinterGameObjects)
                {
                    winterGameObject.SetActive(false);
                }
                break;
            case Season.秋:
                foreach (var sprinGameObject in SprinGameObjects)
                {
                    sprinGameObject.SetActive(false);
                }
                foreach (var autumnGameObject in AutumnGameObjects)
                {
                    autumnGameObject.SetActive(true);
                }
                foreach (var winterGameObject in WinterGameObjects)
                {
                    winterGameObject.SetActive(false);
                }
                break;
            case Season.冬:
                foreach (var sprinGameObject in SprinGameObjects)
                {
                    sprinGameObject.SetActive(false);
                }
                foreach (var autumnGameObject in AutumnGameObjects)
                {
                    autumnGameObject.SetActive(false);
                }
                foreach (var winterGameObject in WinterGameObjects)
                {
                    winterGameObject.SetActive(true);
                }
                break;
            default:
                break;
        }
    }
	// Update is called once per frame
	void Update () {
		
	}
}
