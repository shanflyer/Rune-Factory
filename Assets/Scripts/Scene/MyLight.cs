using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
//[RequireComponent(typeof(Light2D))]
public class MyLight : MonoBehaviour
{
    [SerializeField]
    private bool lerpPs;
    [SerializeField]
    private ParticleSystem[] ps;
    ParticleSystem.MainModule[] mainModules;


    [SerializeField]
    public AnimationCurve psCurve;
    [SerializeField]
    private Light2D light2D;
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private bool autoLerpValue;

    [SerializeField]
    public AnimationCurve lerpCurve;

    [SerializeField]
    private bool autoLerpColor;

    [SerializeField]
    [GradientUsage(true)]
    public Gradient lerpColor;

    private float _psValue;
    private float psValue
    {
        get
        {
            return _psValue;
        }
        set
        {
            _psValue = value;
            for(int i = 0; i < mainModules.Length; i++)
            {
                Color mainColor = mainModules[i].startColor.color;
                mainColor.a = value;
                mainModules[i].startColor = mainColor;
            }
        }
    }

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
        if (ps == null)
        {
            ps = gameObject.GetComponentsInChildren<ParticleSystem>(true);
         
        }
#endif 
        if (Application.isPlaying)
            EnvironmentManger.instance.AddMyLight(this);
    }
    private void Awake()
    {
        mainModules = new ParticleSystem.MainModule[ps.Length];
        for (int i = 0; i < ps.Length; i++)
        {
            var main = ps[i].main;
            mainModules[i] = main;
        }
    }
    private void OnDisable()
    {
        if (Application.isPlaying&&GameController.instance!=null&& !SingletonType.Cleared)
        {
            if (!SingletonType.Cleared)
                EnvironmentManger.instance.RemoveMyLight(this); 
        }
      
    }
    public void LerpTimeValue(float timeValue)
    {
        if (lerpPs)
            psValue = psCurve.Evaluate(timeValue);
        if (autoLerpValue)
            intensity = lerpCurve.Evaluate(timeValue);
        if (autoLerpColor)
            color = lerpColor.Evaluate(timeValue);
        if (light2D)
        {
            if (autoLerpValue)
            {
                if (intensity == 0)
                {
                    light2D.enabled = false;
                }
                else
                {
                    light2D.enabled = true;
                }
            }
            else if (autoLerpColor)
            {
                if (color.a == 0)
                {
                    light2D.enabled = false;
                }
                else
                {
                    light2D.enabled = true;
                }
            }

               
        }
           
    }
}