using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCompiler
{
    internal class ErrorBack
    {
        public int Line;
        public string typeError;
        
        public ErrorBack(int Line, string typeError)
        {
            this.Line = Line;
            this.typeError = typeError;
        }
    }

    public class ErrorValue
    {
        public const string SintaxErrorEnd = "Invalid Sintax. Missing operator: ;";
        public const string SintaxErrorEqual = "Invalid Sintax. Missing operator: =";
        public const string SintaxErrorValue = "Invalid Sintax. Incorrect value type";
        public const string SintaxErrorValueZero = "Invalid Sintax. division by 0 invalid";
        public const string SintaxErrorDeclaration = "Invalid Declaration";
    }

    
}
