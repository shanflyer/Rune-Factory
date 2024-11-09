using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
//[RequireComponent(typeof(Light2D))]
public class MyLight : MonoBehaviour
{
    [SerializeField]
    bool blendWeatherLight;
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
    MyLightBase[] myLightBase;

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
            float weatherLight = EnvironmentManger.instance.weatherLight + EnvironmentManger.instance.lightningLight;
            if (light2D)
            { 
                light2D.intensity = blendWeatherLight ? value * weatherLight : value;
            }
            if (myLightBase != null)
            {
                for (int i = 0; i < myLightBase.Length; i++)
                {
                    if (myLightBase[i])
                    {
                        myLightBase[i].Value = blendWeatherLight ? value * weatherLight : value;
                    }
                }
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
            float weatherLight = EnvironmentManger.instance.weatherLight + EnvironmentManger.instance.lightningLight*0.4f;
            if (light2D)
            {
                light2D.color= blendWeatherLight?value* weatherLight : value;
            }
           
            if (myLightBase != null)
            {
                for(int i = 0; i < myLightBase.Length; i++)
                {
                    if (myLightBase[i])
                    {
                        myLightBase[i].Color = blendWeatherLight ? value * weatherLight : value;
                    } 
                } 
            }
            if (spriteRenderer)
            {
                Color color1 = _color;
                float a = _color.a;
                color1*= weatherLight;
                color1.a=a;
                spriteRenderer.color = blendWeatherLight ?color1: _color;
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
#if UNITY_EDITOR
    public void Display(float value)
    {
        if (autoLerpColor)
        {
            if (light2D)
                light2D.color = lerpColor.Evaluate(value);
              
            if (myLightBase != null)
            {
                for(int i = 0; i < myLightBase.Length; i++)
                {
                    if (myLightBase[i])
                        myLightBase[i].Color = lerpColor.Evaluate(value); 
                }
            }
                
        }
       if(autoLerpValue)
        {
            if (light2D)
                light2D.intensity = lerpCurve.Evaluate(value);

            if (myLightBase != null)
            {
                for (int i = 0; i < myLightBase.Length; i++)
                {
                    if (myLightBase[i])
                        myLightBase[i].Value = lerpCurve.Evaluate(value);
                }
            }
        }
    }
#endif
    private void Awake()
    {
        if (ps!=null)
        {
            mainModules = new ParticleSystem.MainModule[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                var main = ps[i].main;
                mainModules[i] = main;
            }
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
#if UNITY_EDITOR
[CustomEditor(typeof(MyLight))]
public class MyLightEditor : Editor
{
    private float value;
    public MyLight myLight => target as MyLight;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        value = EditorGUILayout.Slider(value, 0, 1);
        if (GUILayout.Button("Test"))
        {
            myLight.Display(value);
        }
    }
}
#endif