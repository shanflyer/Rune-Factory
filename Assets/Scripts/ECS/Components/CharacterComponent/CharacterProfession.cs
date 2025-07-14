using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public struct CharacterProfession : IComponentData
{
    public int level;
    public int totalExp;
    public int nowExp;
    public int nowLevelExp;
    public int professionId;
}