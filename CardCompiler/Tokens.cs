using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCompiler
{
    public class Token
    {
        public TypeToken type;
        public Location location;
        public string value;

        public Token(TypeToken type, Location location, string value)
        {
            this.type = type;
            this.location = location;
            this.value = value;
        }
        public override string ToString()
        {
            return string.Format("{0} [{1}]", type, value);
        }
    }


    public enum TypeToken
    {
        Any,
        Error,
        Keyword,
        Int,
        String,
        Identifier,
        Symbol
    }

    public struct Location
    {
        public int Line;
        public int Column;
        public string File;
    }

    public class TokenValues
    {
        protected TokenValues() { }

        public const string Add = "Addition"; // +
        public const string Sub = "Subtract"; // -
        public const string Mul = "Multiplication"; // *
        public const string Div = "Division"; // /
        public const string Pot = "Potencia"; // ^
        public const string Increment = "Increment"; // ++

        public const string Assign = "Assign"; // =
        public const string ValueSeparator = "ValueSeparator"; // ,
        public const string StatementSeparator = "StatementSeparator"; // ;

        public const string OpenBracket = "OpenBracket"; // (
        public const string ClosedBracket = "ClosedBracket"; // )
        public const string OpenCurlyBraces = "OpenCurlyBraces"; // {
        public const string ClosedCurlyBraces = "ClosedCurlyBraces"; // }

        public const string CompareSup = "Sup"; // >
        public const string CompareMin = "Min"; // <
        public const string CompareSupEqual = "SupE"; // >=
        public const string CompareMinEqual = "MinE"; // <=
        public const string CompareEqual = "Equal"; //==

        public const string Concaten = "Concaten"; //@
        public const string ConcatenSeparator = "ConcatenSeparator"; //@@

        public const string OrLogic = "OrLogic"; // ||
        public const string AndLogic = "AndLogic"; // &&


    }
}
