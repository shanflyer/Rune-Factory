using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NewGame
{
    public class ItemData : ScriptableObject, IGameData
    {
        public int id;
        public string itemName;
        public string icon;
        public string info;
        public List<int> dropEventId = new List<int>(), checkEventId = new List<int>(), useEventId = new List<int>(), equipEventId = new List<int>();
        public int groupCount;

        public override string ToString()
        {
            return id.ToString();
        }
#if UNITY_EDITOR
        public void SetReferenceData()
        {
        }

        public string GetKey()
        {
            return id.ToString() ;
        }
#endif
    }
}

