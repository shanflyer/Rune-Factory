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
    MyLightBase[] myLightBase;

    [SerializeField]
    public SpriteRenderer[] spriteRenderers;

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
            try
            {
                for (int i = 0; i < mainModules.Length; i++)
                {

                    Color mainColor = mainModules[i].startColor.color;
                    mainColor.a = value;
                    mainModules[i].startColor = mainColor;
                }
            }
            catch
            {

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
            float weatherLight = 0;
            if (Application.isPlaying)
            {
                weatherLight = EnvironmentManger.instance.weatherLight + EnvironmentManger.instance.lightningLight;
                weatherLight *= GameTimeManager.instance.timeLightValue;
            }
                
            
            float trueValue= blendWeatherLight ? value * weatherLight : value;
            if (myLightBase != null)
            {
                for (int i = 0; i < myLightBase.Length; i++)
                {
                    if (myLightBase[i])
                    {
                        myLightBase[i].Value = trueValue;
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
            float weatherLight = 0;
            if (Application.isPlaying)
                weatherLight = EnvironmentManger.instance.weatherLight + EnvironmentManger.instance.lightningLight*0.4f;
            
           
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
            if (spriteRenderers!=null)
            {
                
                Color color1 = _color; 
                color1*= weatherLight;
                color1= blendWeatherLight ? color1 : _color;
                if (!blendWeatherLight)
                {
                    color1.a = color1.a * 0.5f;
                }
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    spriteRenderers[i].color= color1;
                } 
            }

        }
    }
    private Color _color;
    private float _intensity;

    private void OnEnable()
    {
#if UNITY_EDITOR
       
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
            if (myLightBase != null)
            {
                for(int i = 0; i < myLightBase.Length; i++)
                {
                    if (myLightBase[i])
                        myLightBase[i].Color = lerpColor.Evaluate(value); 
                }
            }

            if (spriteRenderers != null)
            { 
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    spriteRenderers[i].color = lerpColor.Evaluate(value);
                }
            }

        }
       if(autoLerpValue)
        {
             
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