using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
[System.Serializable]
public enum Arm 
{
    步兵 = 1,
    枪兵 = 2,
    卫兵 = 3,
    法师 = 7,
    神官 = 4,
    飞兵 = 5,
    灵魂 = 6,
    水兵 = 8,
    不死 = 9,
    野伏=10,
    特骑=11,
    魔狼=12,
    特殊=13,
    魔族=14,
    海兽=15,
    射手=16,
    远程=17,
    骑兵=18
}
[System.Serializable]
public class ProfessionLevelData
{
    public int level;
    public int AT;
    public int DF;
    public int MaxHp, MaxPower, NeedEXP;
    public int Crit, Dodge;
}
[System.Serializable]
public class SoliderProfesson
{
    public string name;
    public int id;
}
[System.Serializable]
public class ProfessionData
{
    public string name;
    public string EnglighName;
    public int id;
   
    public Property ZeroProperty;

    public string ZeroPropertyStr;
        //ZeroHp, ZeroPower, ZeroAt, ZeroDf,ZeroNeedExp,ZeroRewardExp, ZeroCrit, ZeroDodge;
    public string attackAudio, unAttackAudio;
    [HideInInspector]
    public int AtPlus, DfPlus,HpPlus,PowerPlus,needExpPlus,rewardExpPlus;
    public Arm arm;
    public int skillId;
    public string hitEffectName;

    public void InitZeroProperty()
    {
        var x = ZeroPropertyStr.Split(',');
        ZeroProperty=new Property();
        ZeroProperty.HP = ZeroProperty.MaxHP = int.Parse(x[0]);
        ZeroProperty.Power = ZeroProperty.MaxPower = int.Parse(x[1]);
        ZeroProperty.AT = int.Parse(x[2]);
        ZeroProperty.DF = int.Parse(x[3]);
        ZeroProperty.NeedEXP = int.Parse(x[4]);
        ZeroProperty.rewardEXP = int.Parse(x[5]);
        ZeroProperty.Crit = int.Parse(x[6]);
        ZeroProperty.Dodge = int.Parse(x[7]); 
    }
    public int GetAT(int level)
    {
        int at = 0;
        at = AtPlus * (level - 1);
        return at;
    }
    public int GetDF(int level)
    {
        int df = 0;
        df = DfPlus * (level - 1);
        return df;
    }
    public int GetMaxHp(int level)
    {
        int MaxHp = 0;
        MaxHp= HpPlus * (level - 1);
        return MaxHp;
    }
    public int GetMaxPower(int level)
    {
        int MaxPower = 0;
        MaxPower = PowerPlus * (level - 1);
        return MaxPower;
    }
    public int GetMaxNeedExp(int level)
    {
        int NeedExp = 0;
        NeedExp = needExpPlus * (level - 1);
        return NeedExp;
    }
    public int GetMaxRewardExp(int level)
    {
        int ReswardExp = 0;
        ReswardExp = rewardExpPlus * (level - 1);
        return ReswardExp;
    }
    public Property GetPropertyFromLevelup(int _level)
    {
        Property newProperty = new Property
        {
            AT = AtPlus,
            DF = DfPlus,
            MaxHP = HpPlus,
            MaxPower = PowerPlus,
            NeedEXP = needExpPlus,
            rewardEXP = rewardExpPlus
        };
        if (_level % 3 == 0)
        {
            newProperty.Crit = 1;
            newProperty.Dodge = 1;
        }
        else
        {
            newProperty.Crit = 0;
            newProperty.Dodge = 0;
        }
        return newProperty;
    }
    public Property GetPropertyFromLevel(int _level)
    {
        Property newProperty = new Property
        {
            AT = AtPlus * (_level - 1),
            DF = DfPlus * (_level - 1),
            MaxHP = HpPlus * (_level - 1),
            MaxPower = PowerPlus * (_level - 1),
            NeedEXP = needExpPlus * (_level - 1),
            rewardEXP = rewardExpPlus * (_level - 1),
            Crit = _level / 3,
            Dodge = _level / 3
        };
        return newProperty;
    }
}
public struct BehaveData
{
    public Animator animator;
    public List<Cell> roadCells;
    public Charactor attackGold;
    public Cell goldCell;
    public Cell OldPos;
    public BehaveData(Charactor charactor)
    {
        animator = charactor.Obj.transform.GetChild(0).GetComponent<Animator>();
        roadCells = new List<Cell>();
        attackGold = null;
        goldCell = null;
        OldPos = null;
    }
}

public class Charactor :MyGameObject
{
    public ProfessionData professionData;
    public int GeneralId;
    public List<Cell> AttackCells;
    public List<Cell> AttackRangeCells;
    public List<Cell> rangeCells;
    public Cell cell;
    public string modeValue;
    public int MV;
    public Property property;
    public Vector3 distanceHp;
    public bool isMagicHurt;
    public GameObject HpGameObject;
    public DeskAction deskAction;
    public NPCAnimationAction npcAnimationAction;
    public int level;
    
   // public CharactorHpData charactorHpData;
   public Charactor() { }
    
    public Charactor(int _id,string _name,GameObject _Obj,int _mapId,Vector2Int _coordinate,
        ProfessionData _professionData,int _level):base(_id,_Obj,_name,_mapId,_coordinate)
    {
        professionData = _professionData;
        cell = AStarTest.GetCellWithCoordinate(_coordinate);
        level = _level;
        if (Obj != null)
        {
            npcAnimationAction = Obj.GetComponent<NPCAnimationAction>();
        }
    }
    public void BultHurt() { }

    public void AttackWaitAnim()
    {
        
    }
    public void AttackAnim()
    {

    }
  
    
    public void SetCell(Cell _cell)
    {
        if (cell != null)
        {
            cell.myGameObjects.Remove(this);
        }
        
        cell = _cell;
        coordinate = _cell.coordinate;
        Obj.transform.position = cell.pos;
        if (!cell.myGameObjects.Contains(this))
        {
            cell.myGameObjects.Add(this);
        }
        
    }
    
    public Cell GetCell()
    {
        return cell;
    }
 
    public List<Cell> GetAttackCells()
    {
        return AttackCells;
    }
    public void SetMapCharactorRange()
    {
        Vector2 monsterCoordinate = coordinate;
        Vector2 monsterPos = AStarTest.CoordinateToPos(monsterCoordinate);
        Obj.transform.position = new Vector3(monsterPos.x, monsterPos.y, 0);
     
        if (monsterCoordinate != new Vector2(-1, -1))
        {
            Cell monsterCell = AStarTest.GetCellWithCoordinate(monsterCoordinate);
            SetCell(monsterCell);
            /*monsterCell.AddCharactor(monster);*/
            rangeCells = AStarTest.CreatRange(monsterCell, MV, this);

            CostRangesCell();
        }
    }
    public void CostRangesCell()
    {
        foreach (var c in rangeCells)
        {
            c.cost = Vector2.Distance(c.coordinate, cell.coordinate);
        }
        rangeCells.Sort();
    }

   
    public Vector2 ReturnAttackAndDefencePlus()
    {
        return Vector2.zero;
    }
    public void CharactorTrans(Vector3 moveV)
    {
        Obj.transform.Translate(new Vector3(moveV.x, moveV.y, 0));
    }
  

   
}
