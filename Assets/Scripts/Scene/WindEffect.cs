using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public struct WindEffectData
{
    public ParticleSystem particleSystem;
    public float2 windSpeed;
    private ParticleSystem.VelocityOverLifetimeModule VelocityOverLifetimeModule;
    bool isInit;
    public void Init()
    {
        if (!isInit)
        {
            isInit = true;
            VelocityOverLifetimeModule = particleSystem.velocityOverLifetime;
            VelocityOverLifetimeModule.x = 0;
        }
           
    }
    public void SetWindValue(float windValue)
    {
        Init();
        float value = (windValue + 1) * 0.5f;
        float speed = math.lerp(windSpeed.x, windSpeed.y, value);
        VelocityOverLifetimeModule.x = speed;
    }
}
public class WindEffect : MonoBehaviour
{
    public List<WindEffectData> effects=new List<WindEffectData>();
    public List<Animator> animators = new List<Animator>();

    private void Awake()
    {
        for(int i = 0; i < effects.Count; i++)
        {
            effects[i].Init();
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
    public void SetWindValue(float windValue)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            effects[i].SetWindValue(windValue);
        }
        for(int i = 0; i < animators.Count; i++)
        {
            animators[i].SetFloat("WindValue", windValue);
        }
    }
    
}
