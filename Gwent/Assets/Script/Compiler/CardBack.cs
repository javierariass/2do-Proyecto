using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCompiler
{
    public class Card
    {
        public string Name;
        public CardType Type;
        public string Faction;
        public int Power;
        public List<AttackType> AttackType;
        public int TriggerPlayer;
        public effect effect;
        public Card(string name, CardType type, string faction, int power,List<AttackType> attackType,int trigerPlayer)
        {
            Name = name;                   
            Type = type;
            Faction = faction;
            Power = power;
            AttackType = attackType;
            TriggerPlayer = trigerPlayer;
        }
    }
    
}
