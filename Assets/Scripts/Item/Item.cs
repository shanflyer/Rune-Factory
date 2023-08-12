using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
namespace NewGame
{
    [System.Serializable]
    public struct Item
    {
        public int instanceId;
        public int dataId;
        public int count;
    }

    public class ItemManager
    {
        public static ItemManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ItemManager();
                }
                return _instance;
            }
        }
        private static ItemManager _instance;

        private List<int> itemIntanceIds = new List<int>();

        public int CreatItemIntance()
        {
            int intanceId = UnityEngine.Random.Range(10000000, 99999999);
            while (itemIntanceIds.Contains(intanceId))
            {
                intanceId = UnityEngine.Random.Range(10000000, 99999999);
            }
            itemIntanceIds.Add(intanceId);
            return intanceId;
        }

        public void DeleteItem(int intanceId)
        {
            if (itemIntanceIds.Contains(intanceId))
            {
                itemIntanceIds.Remove(intanceId);
            }
        }

    }
}
