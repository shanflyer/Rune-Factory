using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using System;
using System.IO; 
using LitJson;
using OldName;
namespace OldName
{ 

    [System.Serializable]
    public class Boundary
    {
        public float MinX, MinY, MaxX, MaxY;
    }
 
    [System.Serializable]
    public struct PackageCaseAdd
    {
        public int caseAdd;
        public int zeroCost;

    }
    public class GameManager : MonoBehaviour
    {
        public Texture2D t1;
        public GameObject ChildFunctionObj;
        public List<Text> FunctionTexts;
        [HideInInspector]
        public ChildData childData;
        public PackageCaseAdd pakageAddData, boxAddData, iceBoxAddData;

        public GameObject IntelligencePanelObj;


        [HideInInspector]
        public Boundary nowBoundary;
        public GameComponent GameData;  

        [HideInInspector]
        public float result0, result1;
        public int passId; 
          
        [HideInInspector] public Vector2Int euqipmentCoordinate;
     


        private void OnNativeShareSuccess(string result)
        {
            // Debug.Log("success: " + result);

        }
        private void OnNativeShareCancel(string result)
        {
            // Debug.Log("cancel: " + result);

        }
        void Start()
        {

        }

        void Awake()
        { 
            GetComponent<GameComponent>().InitData();



            GameData.InitData(); 
             

            result0 = 0;
            result1 = 0; 
        }
        public static void OutputRt(Texture2D rt)
        {
            string str = Application.persistentDataPath + "/001.png";

            byte[] byt = rt.EncodeToPNG();
            File.WriteAllBytes(str, byt);
        }
        void OnApplicationQuit()
        {
            //DataSaveAndLoadTest.gameSaveData.SaveData();
          //  DataSaveAndLoadTest.CreatSaveData(-1);
        }

        public void ChildDateCost()
        {
            //if (DataSaveAndLoadTest.gameSaveData.marryData.haveChildrenTime != null)
            {
                int foodcost = 2;
                int moodcost = 2;
                int cleancost = 2;
                switch (childData.BodyGrowStatus)
                {
                    case GrowStatus.慢速成长:
                        foodcost = 1;
                        break;
                    case GrowStatus.中速成长:
                        foodcost = 2;
                        break;
                    case GrowStatus.生长停滞:
                        foodcost = 0;
                        break;
                    case GrowStatus.高速成长:
                        foodcost = 3;
                        break;
                }
                switch (childData.MindGrowStatus)
                {
                    case GrowStatus.慢速成长:
                        moodcost = 1;
                        break;
                    case GrowStatus.中速成长:
                        moodcost = 2;
                        break;
                    case GrowStatus.生长停滞:
                        moodcost = 0;
                        break;
                    case GrowStatus.高速成长:
                        moodcost = 3;
                        break;
                }
                cleancost = Mathf.RoundToInt((foodcost + moodcost) / 2.0f);
                childData.foodValue -= foodcost;
                childData.moodValue -= moodcost;
                childData.cleanValue -= cleancost;
                if (childData.foodValue < 0)
                {
                    childData.foodValue = 0;
                }
                if (childData.moodValue < 0)
                {
                    childData.moodValue = 0;
                }
                if (childData.cleanValue < 0)
                {
                    childData.cleanValue = 0;
                }
                int bodyGrowValue = Mathf.RoundToInt((childData.foodValue + childData.moodValue) / 2.0f);
                if (childData.foodValue == 0)
                {
                    bodyGrowValue = 0;
                }
                int mindGrowValue = Mathf.RoundToInt((childData.cleanValue + childData.moodValue) / 2.0f);
                if (childData.moodValue == 0)
                {
                    mindGrowValue = 0;
                }
                if (bodyGrowValue >= 8)
                {
                    childData.BodyGrowStatus = GrowStatus.高速成长;
                    childData.bodyValue += 3;
                }
                else if (bodyGrowValue >= 4)
                {
                    childData.BodyGrowStatus = GrowStatus.中速成长;
                    childData.bodyValue += 2;
                }
                else if (bodyGrowValue > 0)
                {
                    childData.BodyGrowStatus = GrowStatus.慢速成长;
                    childData.bodyValue += 1;
                }
                else
                {
                    childData.BodyGrowStatus = GrowStatus.生长停滞;
                }
                if (mindGrowValue >= 8)
                {
                    childData.MindGrowStatus = GrowStatus.高速成长;
                    childData.mindValue += 3;
                }
                else if (mindGrowValue >= 4)
                {
                    childData.MindGrowStatus = GrowStatus.中速成长;
                    childData.mindValue += 2;
                }
                else if (mindGrowValue > 0)
                {
                    childData.MindGrowStatus = GrowStatus.慢速成长;
                    childData.mindValue += 1;
                }
                else
                {
                    childData.MindGrowStatus = GrowStatus.生长停滞;
                }
            }



        }
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
               // DataSaveAndLoadTest.gameSaveData.SaveData();
               // DataSaveAndLoadTest.CreatSaveData(-1);
            }
            else
            {
                CheckPauseTime();

            }
        }
         
        public void CheckPauseTime()
        {
           
        }
          

        // Update is called once per frame
        void Update()
        {

        }
    }
}

