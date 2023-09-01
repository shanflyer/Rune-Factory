using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(FightEventAsset))]
public class FightEventTrack : TrackAsset
{
    public int skill;
    [SerializeField]
    public SkillEstimateData skillActionData;
    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        
        base.GatherProperties(director, driver);
    }
    protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
    {
        SetSkillActionData();
        return base.CreatePlayable(graph, gameObject, clip);
    }

    void SetSkillActionData()
    { 
        foreach (var clip in GetClips())
        {
            var fightEventAsset = clip.asset as FightEventAsset;

            if (fightEventAsset == null)
                continue;

            fightEventAsset.SetSkillActionData(skillActionData);

        }
    }
}