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
        Dictionary<string, string> Symbol = new Dictionary<string, string>();
        Dictionary<string, string> KeyWord = new Dictionary<string, string>();
        List<CardType> cardType = new List<CardType>();
        List<AttackType> attackType = new List<AttackType>();

        public LexicalProcess()
        {
            //Agregadndo Simbolos y Operadores
            AddSymbol("+", TokenValues.Add);
            AddSymbol("-", TokenValues.Sub);
            AddSymbol("*", TokenValues.Mul);
            AddSymbol("/", TokenValues.Div);
            AddSymbol("^", TokenValues.Pot);
            AddSymbol("++", TokenValues.Increment);
            AddSymbol("=", TokenValues.Assign);
            AddSymbol(",", TokenValues.ValueSeparator);
            AddSymbol(";", TokenValues.StatementSeparator);
            AddSymbol("(", TokenValues.OpenBracket);
            AddSymbol(")", TokenValues.ClosedBracket);
            AddSymbol("{", TokenValues.OpenCurlyBraces);
            AddSymbol("}", TokenValues.ClosedCurlyBraces);
            AddSymbol(">", TokenValues.CompareSup);
            AddSymbol("<", TokenValues.CompareMin);
            AddSymbol(">=", TokenValues.CompareSupEqual);
            AddSymbol("<=", TokenValues.CompareMinEqual);
            AddSymbol("==", TokenValues.CompareEqual);
            AddSymbol("@", TokenValues.Concaten);
            AddSymbol("@@", TokenValues.ConcatenSeparator);
            AddSymbol("||", TokenValues.OrLogic);
            AddSymbol("&&", TokenValues.AndLogic);

            cardType.Add(CardType.Oro);
            cardType.Add(CardType.Plata);
            cardType.Add(CardType.Clima);
            cardType.Add(CardType.Aumento);
            cardType.Add(CardType.Lider);

            attackType.Add(AttackType.Melee);
            attackType.Add(AttackType.Ranged);
            attackType.Add(AttackType.Asedio);

            AddKey("card", TokenValues.Card);
            AddKey("effect", TokenValues.Effect);
        }

        public void AddSymbol(string Symb, string type)
        {
            Symbol[Symb] = type;
        }

        public void AddKey(string Key, string type)
        {
            KeyWord[Key] = type;
        }

        public TypeToken VerifyValidate(string Text)
        {
            if(CompareKeyword(Text))
            {
                Console.WriteLine("{0} es una palabra clave", Text);
                return TypeToken.Keyword;
            }
            else if (CompareSymbol(Text))
            {
                Console.WriteLine("{0} es un operador", Text);
                return TypeToken.Symbol;
            }
            else if (Regex.IsMatch(Text, @"^[\'][a-zA-Z' '0-9]*'"))
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

        public bool CompareSymbol(string key)
        {
            foreach (var word in Symbol.Keys)
            {
                if (key == word)
                {
                    return true;
                }
            }
            return false;

        }

        public bool CompareKeyword(string key)
        {
            foreach (var word in KeyWord.Keys)
            {
                if (key == word)
                {
                    return true;                   
                }
            }
            return false;
        }

        public bool CompareCardType(string type)
        {
            foreach(CardType s in cardType)
            {
                if(s.ToString() == type)
                {
                    return true;
                }
            }

            return false;
        }

        public bool CompareAttackType(string type)
        {
            foreach (AttackType s in attackType)
            {
                if (s.ToString() == type)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
