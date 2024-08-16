using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCompiler
{
    internal class CardBack
    {
        private string Name;
        private CardType Type;
        private string Faction;
        private int Power;
        private AttackType AttackType;
        public CardBack(string name, CardType type, string faction, int power,AttackType attackType)
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
        Asedio
    }
    public enum CardType
    {
        Oro,
        Plata,
        Clima,
        Aumento,
        Lider
    }
}
