using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(PolygonCollider2D))]
public class Audio2DPolygon : MonoBehaviour
{
    [SerializeField]
    AudioSource audioSource;
    [SerializeField]
    PolygonCollider2D polygonCollider;
    [SerializeField]
    float2 distanceMap;
    [SerializeField]
    float2 volumeMap;
    private void OnEnable()
    {
        audioSource.mute = true;
        audioSource.volume = 0;
        audioSource.enabled = true;
      
        EnvironmentManger.instance.AddAudio2DPolygon(this);
        audioSource.Play();
    }
    private void OnDisable()
    {
        audioSource.mute = true;
        if (!SingletonType.Cleared)
        {
            EnvironmentManger.instance.RemoveAudio2DPolygon(this);
        }
       
    }
    public void RefreshAudio(Collider2D collider)
    {
        audioSource.mute =false;
        if (collider != null)
        {
            var distance2D = polygonCollider.Distance(collider);
            if (distance2D.isValid)
            {
                
                float distanceValue = (distance2D.distance - distanceMap.x) / (distanceMap.y - distanceMap.x);
                distanceValue = math.clamp(distanceValue, 0, 1);
                audioSource.volume=math.lerp(volumeMap.x,volumeMap.y,distanceValue);
            }
            else
            {
                audioSource.volume = volumeMap.y;
            }
        }
        else
        {
            audioSource.volume = volumeMap.y;
        }
    }
    
}
