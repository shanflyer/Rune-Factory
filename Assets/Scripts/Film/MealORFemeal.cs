using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MealORFemeal : MonoBehaviour
{
    public GameObject meal, femeal;
	// Use this for initialization
	void Start ()
	{
	    if (PlayerDate.gender == Gender.male)
	    {
	        meal.SetActive(true);
            femeal.SetActive(false);
	    }
	    else
	    {
	        meal.SetActive(false);
	        femeal.SetActive(true);
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
