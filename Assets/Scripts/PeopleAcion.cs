using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace OldName
{
    public class PeopleAcion : MonoBehaviour
    {
        public List<GameObject> peoplePros;
        public Transform peopleParent;
        private Charactor npcCharactor;
        public float costTime;

        public List<Charactor> NpcCharactors;
        // Use this for initialization
        void Awake()
        {
            NpcCharactors = new List<Charactor>();
        }
        void Start()
        {


        }

        public void CreatNPC(Vector2Int startCoordinate, Vector2Int endCoordinate, Vector2Int goldCoordinate, float _shopingTime
          , DeskAction _deskAction)
        {

            ProfessionData npcProfessionData = CharactorDataAction.professionDatas[0];
            Vector3 pos = AStarTest.CoordinateToPos(startCoordinate);
            int index = Random.Range(0, peoplePros.Count);
            GameObject npcObj = Instantiate(peoplePros[index], pos, Quaternion.identity);
            npcObj.transform.SetParent(peopleParent);
            npcCharactor = new Charactor(npcProfessionData.id * 1000 + NpcCharactors.Count, "npc", npcObj, 1000,
                startCoordinate, npcProfessionData, 1
            )
            { deskAction = _deskAction };
            NpcCharactors.Add(npcCharactor);
            NPCAnimationAction npcAnimationAction = npcObj.GetComponent<NPCAnimationAction>();
            npcAnimationAction.InitNPCAnimationData(npcCharactor, costTime, _shopingTime, endCoordinate, goldCoordinate);
        }

        public void CleraAllNPC()
        {
            while (NpcCharactors != null && NpcCharactors.Count > 0)
            {
                DestoryCharactory(NpcCharactors[0]);
            }

        }
        public void DestoryCharactory(Charactor _charactor)
        {
            Destroy(_charactor.Obj);
            NpcCharactors.Remove(_charactor);
        }
        // Update is called once per frame
        void Update()
        {

        }
    }

}
