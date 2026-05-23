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
    private GameObjectCurveController.FrameTaskHandle moveSelectButtonHandle;

    public override void SetPanelUISerializeObj()
    {
        nameText = FindChildGameObject<Text>("Text");
        button = GetComponent<Button>();
        secondParent = FindChildGameObject("SecondParent");
        base.SetPanelUISerializeObj();
    }

    private const float showTime = 1.0f;
    private float secondButtonHigh;

    private void StartMoveSelectButtonTask(bool hide)
    {
        if (moveSelectButtonHandle.IsValid)
        {
            GameObjectCurveController.instance.Cancel(moveSelectButtonHandle);
            moveSelectButtonHandle = default;
        }

        button.interactable = false;
        float timeValue = 0;
        List<Vector3> targets = new List<Vector3>(secondSelectButtons.Count);
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

        List<Vector3> starts = new List<Vector3>(secondSelectButtons.Count);
        for (int i = 0; i < secondSelectButtons.Count; i++)
        {
            RectTransform rectTransform = secondSelectButtons[i].transform as RectTransform;
            starts.Add(rectTransform.localPosition);
        }

        moveSelectButtonHandle = GameObjectCurveController.instance.StartFrameTask(deltaTime =>
        {
            float lerpValue = Mathf.Clamp01(timeValue / showTime);
            for (int i = 0; i < secondSelectButtons.Count; i++)
            {
                var selectButton = secondSelectButtons[i];
                Vector3 pos = Vector3.Lerp(starts[i], targets[i], lerpValue);
                RectTransform rectTransform = selectButton.transform as RectTransform;
                rectTransform.localPosition = pos;
            }

            timeValue += deltaTime;
            return timeValue <= showTime;
        }, () =>
        {
            moveSelectButtonHandle = default;
            for (int i = 0; i < secondSelectButtons.Count; i++)
            {
                RectTransform rectTransform = secondSelectButtons[i].transform as RectTransform;
                rectTransform.localPosition = targets[i];
            }

            if (hide)
            {
                secondParent.transform.localScale = Vector3.zero;
            }

            button.interactable = true;
        });
    }

    private void ShowSecondSelectButton()
    {
        show = true;
        StartMoveSelectButtonTask(false);
    }

    private void HideSecondSelectButton()
    {
        show = false;
        StartMoveSelectButtonTask(true);
    }

    public async void InitFunction(FunctionData functionData, Button secondSelectButton)
    {
        this.functionData = functionData;
        nameText.text = functionData.buttonName;
        secondButtonHigh = secondSelectButton.GetComponent<RectTransform>().sizeDelta.y;

        if (functionData.secondFunctions != null && functionData.secondFunctions.Length > 0)
        {
            secondSelectButtons.Clear();
            for (int i = 0; i < functionData.secondFunctions.Length; i++)
            {
                int functionIndex = i;
                var async = InstantiateAsync(secondSelectButton, secondParent, Vector3.zero, Quaternion.identity);
                await async;
                var selectButton = async.Result[0];
                selectButton.transform.localScale = Vector3.one;
                selectButton.GetComponentInChildren<Text>().text = functionData.secondFunctions[functionIndex].buttonName;
                selectButton.onClick.AddListener(() =>
                {
                    functionData.secondFunctions[functionIndex].gameActionData.Action();
                });
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

    public override void OnDisable()
    {
        if (moveSelectButtonHandle.IsValid)
        {
            GameObjectCurveController.instance.Cancel(moveSelectButtonHandle);
            moveSelectButtonHandle = default;
        }

        if (button != null)
        {
            button.interactable = true;
        }
    }
}
