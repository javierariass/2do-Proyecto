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
        List<SourceType> Source = new();

        //Constructor con parametro predefinidos
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
            attackType.Add(AttackType.Siege);

            Source.Add(SourceType.hand);
            Source.Add(SourceType.otherhand);
            Source.Add(SourceType.field);
            Source.Add(SourceType.otherfield);
            Source.Add(SourceType.deck);
            Source.Add(SourceType.otherdeck);
            Source.Add(SourceType.board);

        }

        //Revision de token valido
        public TypeToken VerifyValidate(string Text)
        {
            if (Regex.IsMatch(Text, @"^[\'][a-zA-Z' '0-9]*'"))
            {
                return TypeToken.String;
            }
            else if (Regex.IsMatch(Text, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
            {
                return TypeToken.Var;
            }
            else if (Regex.IsMatch(Text, @"^-?\d+$"))
            {
                return TypeToken.Number;
            }            
            else
            {
                return TypeToken.Error;
            }
            
            
        }

        //Verificar existencia del tipo carta pasado
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

        //Verificar existencia del tipo de ataque pasado
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

        //Verificar existencia del tipo source selector pasado
        public string CompareSourceType(string type)
        {
            foreach (SourceType s in Source)
            {
                if (s.ToString() == type)
                {
                    return s.ToString();
                }
            }

            return " ";
        }
    }


    //Definicion de enum caracteristicos del compiler

    //Tipos de Variable
    public enum TypeToken
    {
        Error,
        Number,
        String,
        Var
    }
    //Tipos de ataque
    public enum AttackType
    {
        Melee,
        Ranged,
        Siege,
        None
    }
    //Tipos de cartas
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
    //Tipos de Source Selector
    public enum SourceType
    {
        hand,
        otherhand,
        field,
        otherfield,
        deck,
        otherdeck,
        board
    }
}
