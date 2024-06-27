using System.Collections; 
using UnityEngine;
using static UnityEngine.ParticleSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
 
public class FishParticle
{
    public float speed;
    public float moveValue;
    public Vector3 startPos;
    public Vector3 targetPos;
    public bool isDeath;
    public float waitTime;
}

public class FishTool : MonoBehaviour
{
    public ParticleSystem particleSystem;
    private Particle[] m_Particles;
    private int numParticlesAlive; 
    public Vector2 speedRange;
    public Transform target;
    public AnimationCurve moveCurve;

    private FishParticle fishParticle;

    float minSize, maxSize;

    public float FishValue => fishValue;
    public bool isGetFish => fishGetValue >= 0;
    private float fishValue,fishGetValue;
    private void Awake()
    {
        var main = particleSystem.main;
        minSize = main.startSize.constantMin;
        maxSize = main.startSize.constantMax;
    }
    public void StartFishing()
    {
        ParticleSystem.EmitParams ep = new ParticleSystem.EmitParams();
        particleSystem.Emit(ep, 1);
        m_Particles = new Particle[5];
        numParticlesAlive = particleSystem.GetParticles(m_Particles);

        if (numParticlesAlive > 0)
        {
            fishParticle = new FishParticle
            {
                speed = Random.Range(speedRange.x, speedRange.y),
                startPos = m_Particles[numParticlesAlive - 1].position,
                targetPos = target.position - particleSystem.transform.position
            };
            fishValue = (m_Particles[numParticlesAlive - 1].startSize - minSize) / (maxSize - minSize); 
        }

        var noise = particleSystem.noise;
        noise.enabled = false;
        StopAllCoroutines();
        StartCoroutine(MoveFish());
    }

    private IEnumerator MoveFish()
    {
        bool AllParticleDeath = false;
        fishGetValue = 0;
        while (!AllParticleDeath)
        {
            AllParticleDeath = true;
            numParticlesAlive = particleSystem.GetParticles(m_Particles);
            if (numParticlesAlive == 0)
            {
                break;
            }

            if (!fishParticle.isDeath)
            {
                if (fishParticle.moveValue < 1)
                {
                    fishParticle.moveValue += Time.deltaTime * fishParticle.speed;
                    fishGetValue = moveCurve.Evaluate(fishParticle.moveValue);
                    if (fishGetValue > 1)
                    {
                        fishGetValue = 1;
                    }
                    Vector3 pos = Vector3.Lerp(fishParticle.startPos, fishParticle.targetPos, fishGetValue);
                    m_Particles[numParticlesAlive - 1].position = pos;
                }
                else
                {
                    fishParticle.isDeath = true;
                    var noise = particleSystem.noise;
                    noise.enabled = true;
                }
            }
            if (!fishParticle.isDeath)
            {
                AllParticleDeath = false;
            }
            particleSystem.SetParticles(m_Particles, numParticlesAlive);

            yield return 0;
        }
    }

    public void StopFishing()
    {
        if (isGetFish)
        {
            particleSystem.Stop();
        }
        else
        {
            var noise = particleSystem.noise;
            noise.enabled = true; 
        }
        fishGetValue = 0;
        StopAllCoroutines();
    }

     
}
#if UNITY_EDITOR
[CustomEditor(typeof(FishTool))]
public class FishToolEditor : Editor
{
    public FishTool test
    {
        get
        {
            return (FishTool)target;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("≤‚ ‘"))
        {
            test.StartFishing();
        }
        if (GUILayout.Button("Õ£÷π"))
        {
            test.StopFishing();
        }
    }
}
#endif
