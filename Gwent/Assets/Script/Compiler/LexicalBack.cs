using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardCompiler
{
    public class LexicalProcess
    {
        List<CardType> cardType = new();
        List<AttackType> attackType = new();
        List<string> Source = new();

        public LexicalProcess()
        {

            cardType.Add(CardType.Oro);
            cardType.Add(CardType.Plata);
            cardType.Add(CardType.Clima);
            cardType.Add(CardType.Aumento);
            cardType.Add(CardType.Lure);
            cardType.Add(CardType.Despeje);
            cardType.Add(CardType.Lider);

            attackType.Add(AttackType.Melee);
            attackType.Add(AttackType.Ranged);
            attackType.Add(AttackType.Asedio);

            Source.Add("hand");
            Source.Add("otherhand");
            Source.Add("field");
            Source.Add("otherfield");
            Source.Add("deck");
            Source.Add("otherdeck");
            Source.Add("board");

        }


        public TypeToken VerifyValidate(string Text)
        {
            if (Regex.IsMatch(Text, @"^[\'][a-zA-Z' '0-9]*'"))
            {
                Console.WriteLine("{0} es un string", Text);
                return TypeToken.String;
            }
            else if (Regex.IsMatch(Text, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
            {
                Console.WriteLine("{0} es una variable valida", Text);
                return TypeToken.Var;
            }
            else if (Regex.IsMatch(Text, @"^-?\d+$"))
            {
                Console.WriteLine("{0} es un numero", Text);
                return TypeToken.Number;
            }
            
            else
            {
                Console.WriteLine("{0} sintax error", Text);
                return TypeToken.Error;
            }
            
            
        }

        public CardType CompareCardType(string type)
        {
            foreach(CardType s in cardType)
            {
                if(s.ToString() == type)
                {
                    return s;
                }
            }

            return CardType.None;
        }

        public AttackType CompareAttackType(string type)
        {
            foreach (AttackType s in attackType)
            {
                if (s.ToString() == type)
                {
                    return s;
                }
            }

            return AttackType.None;
        }

        public string CompareSourceType(string type)
        {
            foreach (string s in Source)
            {
                if (s == type)
                {
                    return s;
                }
            }

            return " ";
        }
    }
}
