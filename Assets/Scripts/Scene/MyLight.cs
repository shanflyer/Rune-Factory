using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
//[RequireComponent(typeof(Light2D))]
public class MyLight : MonoBehaviour
{
    [SerializeField]
    private Light2D light2D;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private bool autoLerpValue;

    [SerializeField]
    private AnimationCurve lerpCurve;

    [SerializeField]
    private bool autoLerpColor;

    [SerializeField]
    [GradientUsage(true)]
    private Gradient lerpColor;
    

    [SerializeField]
    private float intensity
    {
        get
        {
            return _intensity;
        }
        set
        {
            _intensity = value;
            if (light2D)
            {
                light2D.intensity = value;
            }
        }

         
    }

    [SerializeField]
    private Color color
    {
        get
        {
            return _color;  
        }

        
        set
        {
            _color = value;
            if (light2D)
            {
                light2D.color= value;
            }
            if (spriteRenderer)
            {
                spriteRenderer.color = _color;
            }

        }
    }
    private Color _color;
    private float _intensity;

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (light2D == null)
        {
            light2D = GetComponent<Light2D>();
            if(light2D!=null)
                _intensity = light2D.intensity;
        }
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
#endif 
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