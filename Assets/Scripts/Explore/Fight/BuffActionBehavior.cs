using System; 
using UnityEngine;
[ExecuteAlways]
 public class BuffActionBehavior:MonoBehaviour
 {
    [SerializeField]
    private ParticleSystem particleSystem;
    public Action stopAction;

    public void PlayParticle()
    {
        if(particleSystem != null)
        {
            particleSystem.Play();
        }
    }
    public void StopParticle()
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
        else
        {
            if (stopAction != null) stopAction(); 
        }
    }
    private void OnParticleSystemStopped()
    {
        if(stopAction != null) stopAction();
    }
}