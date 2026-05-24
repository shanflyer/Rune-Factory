using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FrameAnimationData", menuName = "Scriptable Objects/FrameAnimationData")]
public class FrameAnimationData : ScriptableObject
{
    public int fps;
    public List<SpriteBonePose> bones;
}
