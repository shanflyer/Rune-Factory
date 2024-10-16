using Unity.Mathematics; 
using UnityEngine;

[CreateAssetMenu(menuName ="Data/雷电数据")]
public class LightningData : ScriptableObject
{
    public float2 lightningCD;
    public float LightningTime;
    public float2 lightningSpeed;
    public float2 waitSoundTime;
    public BGS[] sounds; 
    public AnimationCurve lightningCurve;
    [ColorUsageAttribute(true, true)]
    public Color color;
}