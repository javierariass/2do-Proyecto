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
        public Card(string name, CardType type, string faction, int power,List<AttackType> attackType)
        {
            Name = name;
            Type = type;
            Faction = faction;
            Power = power;
            AttackType = attackType;
        }
    }
    public enum AttackType
    {
        Melee,
        Ranged,
        Asedio,
        None
    }
    public enum CardType
    {
        Oro,
        Plata,
        Clima,
        Aumento,
        Lider,
        Despeje,
        Lure,
        None
    }
}
