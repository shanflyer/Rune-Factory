using System;
using System.Globalization;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Playables;
#if !UNITY_2020_2_OR_NEWER
using L10n = UnityEditor.Timeline.L10n;
#endif

namespace UnityEngine.Timeline
{
    [CustomEditor(typeof(ControlTrack))]
    [CanEditMultipleObjects]
    class ControlTrackInspector : TrackAssetInspector
    {
        
        SerializedProperty m_matchDatas; 

        public override void OnEnable()
        {
            base.OnEnable();
             
            m_matchDatas = serializedObject.FindProperty("matchDatas");
        }

        protected override void DrawTrackProperties()
        {
            EditorGUILayout.PropertyField(m_matchDatas, true);
            // Volume
            base.DrawTrackProperties();
        }

         

        protected override void ApplyChanges()
        {
            base.ApplyChanges();
        }

       
    }
}
