using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic; 

[Serializable]
public class WindEffectData
{

    public ParticleSystem particleSystem;
    public float2 windSpeed;
    private int maxParticle;
    public bool valueCurve;
    private ParticleSystem.VelocityOverLifetimeModule VelocityOverLifetimeModule;
    bool isInit; 
    public void Init()
    {
        if (!isInit)
        {
            isInit = true; 
            VelocityOverLifetimeModule = particleSystem.velocityOverLifetime;
            
            maxParticle = particleSystem.main.maxParticles;
            if (!valueCurve)
            {
                VelocityOverLifetimeModule.x = 0;
            }
           // particleSystem.Stop();
            if (particleSystem.particleCount > 0)
            {
                particleSystem.SetParticles(new ParticleSystem.Particle[0], 0);
            }

            
        }
           
    }
    public void SetWindValue(float windValue)
    {
        Init();
        float value = (windValue + 1) * 0.5f;
        float speed = math.lerp(windSpeed.x, windSpeed.y, value);
        if (valueCurve)
        {
          
            var curve = VelocityOverLifetimeModule.x.curve;
            var keys = curve.keys;
            keys[0].value = speed;
            curve.keys = keys;
            VelocityOverLifetimeModule.x = new ParticleSystem.MinMaxCurve(1, curve);
        }
        else
        {
            VelocityOverLifetimeModule.x = speed;
        }

        if (particleSystem.isStopped)
        {
            particleSystem.Play();
        }
        
    }
    public void SetSeason(float seasonValue)
    {
        Init();
        if (particleSystem.isStopped)
        {
            particleSystem.Play();
        }
        var main=particleSystem.main;
        main.maxParticles =(int) (maxParticle * seasonValue);

        if (particleSystem.particleCount > main.maxParticles)
        {
            ParticleSystem.Particle[] Particle = new ParticleSystem.Particle[particleSystem.particleCount];
            particleSystem.GetParticles(Particle);
            if (particleSystem.particleCount > main.maxParticles)
            {
                for(int i = main.maxParticles; i < Particle.Length; i++)
                {
                    Particle[i].remainingLifetime = 0;
                    Particle[i].startLifetime = 0;
                }
                particleSystem.SetParticles(Particle, Particle.Length);
            }
        }
        else
        {
            particleSystem.Play();
        }

       
    }
}
public enum AnimatorWindType
{
    控制参数,动画速度,混合
}
public class WindEffect : MonoBehaviour
{
    public AudioSource audioSource;
    public AnimationCurve audioCurve;
    public List<WindEffectData> effects=new List<WindEffectData>();
    public List<Animator> animators = new List<Animator>();
    [SerializeField]
    float windBlendValue = 1;
    [SerializeField]
    AnimatorWindType AnimatorWindType;
    [SerializeField]
    bool snowWind = true;
    [SerializeField]
    float4 seasonRemap;
    [SerializeField]
    bool seasomBlend;
   
    private void Awake()
    {
        for(int i = 0; i < effects.Count; i++)
        {
            effects[i].Init();
        }
        if (audioSource)
        {
            audioSource.pitch = 1;
        }
    }
    private void OnDisable()
    {
        if (Application.isPlaying && GameController.instance != null && !SingletonType.Cleared)
        {
            if (!SingletonType.Cleared)
                EnvironmentManger.instance.RemoveWindEffect(this);
        }

    }
    private void OnEnable()
    { 
        if (Application.isPlaying)
            EnvironmentManger.instance.AddWindEffect(this);
    }
    public void SetSeasonValue()
    {
        float seasomValue = 0;
        if (seasomBlend)
        {
            float seasomValue0 = 0; float seasomValue1 = 0;
            seasomValue0 = math.remap(seasonRemap.x, seasonRemap.y, 0, seasonRemap.w, GameTimeManager.instance.SeasonValue);
            seasomValue0 = math.clamp(seasomValue0, 0, 1);


            seasomValue1 = math.remap(seasonRemap.y, seasonRemap.z, seasonRemap.w, 0, GameTimeManager.instance.SeasonValue);
            seasomValue1 = math.clamp(seasomValue1, 0, 1);
            if (GameTimeManager.instance.SeasonValue > seasonRemap.y)
            {
                seasomValue = seasomValue1;
            }
            else
            {
                seasomValue = seasomValue0;
            }
            for (int i = 0; i < effects.Count; i++)
            { 
                if (seasomBlend)
                {
                    effects[i].SetSeason(seasomValue);
                }
            }
        }
      
    }
    public void SetWindValue(float windValue)
    {
        windValue *= windBlendValue;
        if (!snowWind)
        {
            float seasonValue = GameTimeManager.instance.SeasonValue;
            bool snow = seasonValue >= 3 || seasonValue < 0.05f;
            if(snow)
            {
                windValue = 0;
            }
            for (int i = 0; i < effects.Count; i++)
            {
                effects[i].SetWindValue(windValue);
                
            }
            switch (AnimatorWindType)
            {
                case AnimatorWindType.控制参数:
                    for (int i = 0; i < animators.Count; i++)
                    {
                        animators[i].SetFloat("WindValue", windValue);
                    }
                    break;
                case AnimatorWindType.动画速度:
                    for (int i = 0; i < animators.Count; i++)
                    {
                        animators[i].speed = math.abs(windValue); 
                    }
                    break;
                case AnimatorWindType.混合:
                    for (int i = 0; i < animators.Count; i++)
                    {
                        animators[i].speed = math.abs(windValue);
                        animators[i].SetFloat("WindValue", windValue);
                    }
                    break;
            }
            
            SetSeasonValue();
        }
        if (audioSource)
        {
            audioSource.pitch = audioCurve.Evaluate(windValue);
        }
    }
    
}
