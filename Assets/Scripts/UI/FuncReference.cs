using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FuncReference : UIObjReference<FunctionData>
{
    [SerializeField]
    private Text nameText;

    [SerializeField]
    private Button button;

    [SerializeField]
    private Transform secondParent;

    private FunctionData functionData;
    private List<Button> secondSelectButtons = new List<Button>();

    public override void SetPanelUISerializeObj()
    {
        nameText = FindChildGameObject<Text>("Text");
        button = GetComponent<Button>();
        secondParent = FindChildGameObject("SecondParent");
        base.SetPanelUISerializeObj();
    }

    private const float showTime = 1.0f;
    private float secondButtonHigh;
    private bool show = false;

    private IEnumerator MoveSelectButton(bool hide)
    {
        button.interactable = false;
        float timeValue = 0;
        List<Vector3> targets = new List<Vector3>();
        if (hide)
        {
            for (int i = 0; i < secondSelectButtons.Count; i++)
            {
                targets.Add(Vector3.zero);
            }
        }
        else
        {
            secondParent.transform.localScale = Vector3.one;
            for (int i = 0; i < secondSelectButtons.Count; i++)
            {
                float y = secondButtonHigh * i + secondButtonHigh;
                targets.Add(new Vector3(0, y, 0));
            }
        }
        List<Vector3> zeros = new List<Vector3>();
        for (int i = 0; i < secondSelectButtons.Count; i++)
        {
            RectTransform rectTransform = secondSelectButtons[i].transform as RectTransform;

            zeros.Add(rectTransform.localPosition);
        }

        while (timeValue <= showTime)
        {
            float lerpValue = timeValue / showTime;
            for (int i = 0; i < secondSelectButtons.Count; i++)
            {
                var selectButton = secondSelectButtons[i];
                Vector3 pos = Vector3.Lerp(zeros[i], targets[i], lerpValue);
                RectTransform rectTransform = selectButton.transform as RectTransform;
                rectTransform.localPosition = pos;
            }

            timeValue += Time.deltaTime;
            yield return 0;
        }
        if (hide)
        {
            secondParent.transform.localScale = Vector3.zero;
        }
        button.interactable = true;
    }

    private void ShowSecondSelectButton()
    {
        show = true;
        IEnumerator enumerator = MoveSelectButton(false);
        GameController.instance.StartCoroutine(enumerator);
    }

    private void HideSecondSelectButton()
    {
        show = false;
        IEnumerator enumerator = MoveSelectButton(true);
        GameController.instance.StartCoroutine(enumerator);
    }

    public void InitFunction(FunctionData functionData, Button secondSelectButton)
    {
        this.functionData = functionData;
        nameText.text = functionData.buttonName;
        secondButtonHigh = secondSelectButton.GetComponent<RectTransform>().sizeDelta.y;

        if (functionData.secondFunctions != null && functionData.secondFunctions.Length > 0)
        {
            secondSelectButtons.Clear();
            for (int i = 0; i < functionData.secondFunctions.Length; i++)
            {
                var selectButton = Instantiate(secondSelectButton, Vector3.zero, Quaternion.identity, secondParent);
                selectButton.transform.localScale = Vector3.one;
                selectButton.GetComponentInChildren<Text>().text = functionData.secondFunctions[i].buttonName;
                selectButton.onClick.AddListener(() => { functionData.secondFunctions[i].gameActionData.Action(); });
                secondSelectButtons.Add(selectButton);
            }
            button.onClick.AddListener(() =>
            {
                if (show)
                {
                    HideSecondSelectButton();
                }
                else
                {
                    ShowSecondSelectButton();
                }
            });
        }
        else
        {
            button.onClick.AddListener(() => { this.functionData.gameActionData.Action(); });
        }
    }
}