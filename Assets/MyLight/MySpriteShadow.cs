using Unity.Mathematics;
using UnityEngine;

public class MySpriteShadow : MonoBehaviour
{
    public float offSet;
    public float offSetAngle;
    public Vector3 childScale0, childScale1;
    private void OnEnable()
    { 
        if (Application.isPlaying)
            EnvironmentManger.instance.AddMyShadow(this);
    }
    private void OnDisable()
    {
        if (Application.isPlaying)
            EnvironmentManger.instance.RemoveMyShadow(this);
    }
    public void SetDirectionAngle(float directionX)
    {
        float value = (directionX + 1) * 0.5f;
        float angle = math.lerp(0, 180, value) + math.lerp(-offSetAngle, offSetAngle, directionX);
        angle = math.clamp(angle, 0, 180);
        transform.localEulerAngles = new Vector3(0, 0, angle);
        transform.GetChild(0).localPosition = new Vector3(offSet * math.abs(directionX), 0, 0);
        Vector3 scale = Vector3.Lerp(childScale0, childScale1, math.abs(directionX));
        transform.GetChild(0).localScale = scale;
    }
}
