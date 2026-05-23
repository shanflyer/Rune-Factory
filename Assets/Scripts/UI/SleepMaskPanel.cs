using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class SleepMaskPanel : GamePanel<IReferenceData> 
{
    [SerializeField]
    Image mask;
    private GameObjectCurveController.FrameTaskHandle maskTaskHandle;
    public override void InitReferenceData(IReferenceData v)
    {
        mask = transform.GetComponent<Image>();
        base.InitReferenceData(v);
    }
    public override Task InitData(string dataKey)
    {
        StartLerpMaskTask();
        return base.InitData(dataKey);
    }

    private void StartLerpMaskTask()
    {
        if (maskTaskHandle.IsValid)
        {
            GameObjectCurveController.instance.Cancel(maskTaskHandle);
            maskTaskHandle = default;
        }

        float timeValue = 0;
        float costTime = Mathf.Max(GameCommon.sleepCostTime, 0.001f);
        float halfCostTime = costTime * 0.5f;
        maskTaskHandle = GameObjectCurveController.instance.StartFrameTask(deltaTime =>
        {
            timeValue += deltaTime;
            if (timeValue < halfCostTime)
            {
                float value = Mathf.Clamp01(timeValue / halfCostTime);
                mask.color = new Color(0, 0, 0, value);
                AudioController.instance.SetBgmAudioSourceVolume(1-value);
            }
            else
            {
                float value = Mathf.Clamp01(1- (timeValue - halfCostTime) / halfCostTime);
                mask.color = new Color(0, 0, 0, value);
                AudioController.instance.SetBgmAudioSourceVolume(1-value);
            }

            return timeValue < costTime;
        }, () =>
        {
            maskTaskHandle = default;
            AudioController.instance.SetBgmAudioSourceVolume(1);
            Close();
        });
    }
}
