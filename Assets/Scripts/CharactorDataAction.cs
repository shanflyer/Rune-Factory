using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
namespace OldName
{
    [System.Serializable]
    public class TileAddition
    {
        public TileType tileType;
        public int barrierMultiply;
        public int ATAddition, DFAddition;
        public int otherAddition;
    }
    [System.Serializable]
    public class ArmAddition
    {
        public Arm arm;
        public int ATAddition, DFAddition;
        public int otherAddition;
    }
    [System.Serializable]
    public class ArmData
    {
        public Arm arm;
        public List<TileAddition> tileAdditions;
        public List<ArmAddition> armAdditions;

        public TileAddition FindTileAdditionForTileType(TileType tileType)
        {
            for (int i = 0; i < tileAdditions.Count; i++)
            {
                if (tileAdditions[i].tileType == tileType)
                {
                    return tileAdditions[i];
                }
            }
            return null;
        }
    }
    [System.Serializable]
    public struct ProfessionObj
    {
        public int professionId;
        public string ObjName;
        public string IconName;

        public List<int> NextProfession;
    }
    [System.Serializable]
    public struct PlayerObjData
    {
        public int playerId;
        public List<ProfessionObj> objDatas;
    }
    public class CharactorDataAction : MonoBehaviour
    {
        public List<ArmData> armDatas0;
        public List<ProfessionData> professionDatas0;

        public static List<ArmData> armDatas;
        public static List<ProfessionData> professionDatas;
        void Start()
        {
            JsonToArmData();
            JsonToProfessionData();
        }

        public void InitData()
        {
            JsonToArmData();
            JsonToProfessionData();
        }
        public static ArmData FindArmDataForArm(Arm _arm)
        {
            for (int i = 0; i < armDatas.Count; i++)
            {
                if (armDatas[i].arm == _arm)
                {
                    return armDatas[i];
                }
            }
            return null;
        }


        public void ArmDataToJson()
        {

            string filePath = Application.dataPath + @"/Resources/Datas/ArmData.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            string jsonStr = JsonMapper.ToJson(armDatas0);
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fileStream);
            sw.Write(jsonStr);
            sw.Close();
        }
        public void JsonToArmData()
        {
            TextAsset fileText = Resources.Load<TextAsset>("Datas/" + "ArmData");
            if (fileText == null)
            {
                // Debug.LogError("No" + "ArmData");
            }
            else
            {
                string jsonStr = fileText.text;
                armDatas0 = JsonMapper.ToObject<List<ArmData>>(jsonStr);
                armDatas = armDatas0;
            }
        }
        public void ProfessionDataToJson()
        {
            string filePath = Application.dataPath + @"/Resources/Datas/ProfessionData.json";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            string jsonStr = JsonMapper.ToJson(professionDatas0);
            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
            StreamWriter sw = new StreamWriter(fileStream);
            sw.Write(jsonStr);
            sw.Close();
        }
        public void JsonToProfessionData()
        {
            TextAsset fileText = Resources.Load<TextAsset>("Datas/" + "ProfessionData");
            if (fileText == null)
            {
                Debug.LogError("No" + "ProfessionData");
            }
            else
            {
                string jsonStr = fileText.text;
                professionDatas0 = JsonMapper.ToObject<List<ProfessionData>>(jsonStr);
            }
            if (Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseTraditional)
            {

            }
            else
            {
                foreach (var professionData in professionDatas0)
                {
                    professionData.name = professionData.EnglighName;
                }

            }
            //ZeroHp, ZeroPower, ZeroAt, ZeroDf,ZeroNeedExp,ZeroRewardExp, ZeroCrit, ZeroDodge;
            foreach (var professionData in professionDatas0)
            {
                professionData.InitZeroProperty();
            }

            professionDatas = professionDatas0;
        }
        // Update is called once per frame
        void Update()
        {

        }
    }

}
