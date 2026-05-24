using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectMasker : MonoBehaviour
{
    [SerializeField]
    private Transform blueMasker, redMasker;

    public void SelectAction(bool selected)
    {
        if (selected)
        {
            if (redMasker)
                redMasker.gameObject.SetActive(true);
            if (blueMasker)
                blueMasker.gameObject.SetActive(false);
        }
        else
        {
            if (redMasker)
                redMasker.gameObject.SetActive(false);
            if (blueMasker)
                blueMasker.gameObject.SetActive(true);
        }
    }
    public void DisplayOrHide(bool display)
    {
        if (display)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            transform.localScale = Vector3.zero;
        }
    }
}
