using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Light2D))]
public class MyLight : MonoBehaviour
{
    [SerializeField]
    private Light2D light2D;


    [SerializeField]
    private bool autoLerpValue;

    [SerializeField]
    private AnimationCurve lerpCurve;

    [SerializeField]
    private bool autoLerpColor;

    [SerializeField]
    private Gradient lerpColor;

    [SerializeField]
    private float intensity
    {
        get => light2D.intensity;
        set
        {
            light2D.intensity = value;
        }
    }

    [SerializeField]
    private Color color
    {
        get => light2D.color;
        set
        {
            light2D.color = value;
        }
    }

    private void OnEnable()
    {
        if (light2D == null)
        {
            light2D = GetComponent<Light2D>();
        }

        if (Application.isPlaying)
            EnvironmentManger.instance.AddMyLight(this);
    }
    private void OnDisable()
    {
        if (Application.isPlaying&&GameController.instance!=null)
        {
            EnvironmentManger.instance.RemoveMyLight(this);
        }
      
    }
    public void LerpTimeValue(float timeValue)
    {
        if (autoLerpValue)
            intensity = lerpCurve.Evaluate(timeValue);
        if (autoLerpColor)
            color = lerpColor.Evaluate(timeValue);
    }
}