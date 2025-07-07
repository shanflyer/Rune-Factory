using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class SleepMaskPanel : GamePanel<IReferenceData> 
{
    [SerializeField]
    Image mask;
    public override void InitReferenceData(IReferenceData v)
    {
        mask = transform.GetComponent<Image>();
        base.InitReferenceData(v);
    }
    public override void InitData(string dataKey)
    {
        StartCoroutine(LerpMask());
        base.InitData(dataKey);
    }
    IEnumerator LerpMask()
    {
        float timeValue = 0;
        float halfCostTime = GameCommon.sleepCostTime * 0.5f;
        while (timeValue < GameCommon.sleepCostTime)
        {
            timeValue += Time.deltaTime;
            if (timeValue < halfCostTime)
            {
                float value=timeValue/ halfCostTime;
                mask.color = new Color(0, 0, 0, value);
                AudioController.instance.SetBgmAudioSourceVolume(1-value);
            }
            else
            {
                float value =1- (timeValue - halfCostTime) / halfCostTime;
                mask.color = new Color(0, 0, 0, value);
                AudioController.instance.SetBgmAudioSourceVolume(1-value);
            }
            AudioController.instance.SetBgmAudioSourceVolume(1);
            yield return 0;
        }
        Close();
    }
}