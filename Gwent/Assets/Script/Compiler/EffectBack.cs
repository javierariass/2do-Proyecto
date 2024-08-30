using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCompiler
{
    public class effect
    {
        public string Name;
        public string Source;
        public List<string> Instruntions;
        public List<string> Local_param;
        public List<string> Targets;
        public Dictionary<string, string> Params;
        
        public effect(string Definicion)
        {

        }
    }
}
