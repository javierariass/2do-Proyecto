using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardCompiler
{
    internal class SintaxBack
    {
        public static Dictionary<string, string> VarString = new Dictionary<string, string>();
        public static Dictionary<string, string> VarInt = new Dictionary<string, string>();
        public static Dictionary<string,string> VarDouble = new Dictionary<string, string>();
        public static Dictionary<string, string> VarBoolean = new Dictionary<string, string>();

        List<Token> tokens = new List<Token>();
        static LexicalProcess process = new LexicalProcess();
        public void VerifcateSintax(string Code)
        {

            List<Token> Tokens = new List<Token>();
            string[] Lines = Code.Split('\n');
            for(int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = EliminateSpace(Lines[i]);
            }
            foreach (string Line in Lines)
            {
                //Verificacion de declaracion string simple
                if (Regex.IsMatch(Line, @"^string|<#texto\s+[a-z](1,15)(\s+:\s+[a-z](1,15)')*;$"))
                {
                    Console.WriteLine("{0}   Es una declaracion string", Line);
                    CreateStringVar(Line);
                }

                //Verificacion de declaracion int simple
                else if (Regex.IsMatch(Line, @"^int|<#integer\s+[a-z](1,15)(\s+:\s+\d(0,32000))*;$") && Find(Line,'='))
                {
                    CreateIntVar(Line);
                }

                //Verificacion de declaracion double simple
                else if (Regex.IsMatch(Line, @"^double|<#real\s+[a-z](1,15)(\s+:\s+\d(0,32000))*;$") && Find(Line, '='))
                {
                    CreateDoubleVar(Line);
                }

                //Verificacion de declaracion bool simple
                if (Regex.IsMatch(Line, @"^bool|<#boolean\s+[a-z](1,15)(\s+:\s+(true|false))*;$") && Find(Line, '='))
                {
                    CreateBoolVar(Line);
                }
            }
        }

        //Metodo para crear una variable booleana
        private static void CreateBoolVar(string Line)
        {
            string[] separavar = Line.Split('=');
            string[] Verify = separavar[0].Split(' ');
            string[] ent = separavar[1].Split(';');
            if (Verify.Length >= 2 && process.VerifyValidate(Verify[1]) == TypeToken.Var)
            {
                try
                {
                    bool var;
                    var = bool.Parse(ent[0]);
                    VarBoolean[Verify[1]] = var.ToString();
                    Console.WriteLine("{0} si es una variable bool", var);

                }
                catch (FormatException)
                {
                    Console.WriteLine("no es una variable bool");

                }
            }
            else
            {
                Console.WriteLine("Sintax Invalid.");
            }
        }

        //Metodo para crear una variable double
        private static void CreateDoubleVar(string Line)
        {
            string[] separanum = Line.Split('=');
            string[] Verify = separanum[0].Split(' ');
            string[] ent = separanum[1].Split(';');
            if (Verify.Length >= 2 && process.VerifyValidate(Verify[1]) == TypeToken.Var)
            {
                try
                {
                    double num;
                    num = double.Parse(ent[0]);
                    VarDouble[Verify[1]] = num.ToString();
                    Console.WriteLine("{0} si es un numero double", num);
                }
                catch (FormatException)
                {
                    Console.WriteLine("{0} no es un numero double", ent[0]);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("error de escritura");
                }
            }
            else
            {
                Console.WriteLine("Sintax Invalid.");
            }
        }

        //Metodo para crear una variable int
        private static void CreateIntVar(string Line)
        {
            string[] separanum = Line.Split('=');
            string[] Verify = separanum[0].Split(' ');
            string[] ent = separanum[1].Split(';');
            if(Verify.Length >= 2 && process.VerifyValidate(Verify[1]) == TypeToken.Var)
            {
                try
                {
                    int num;
                    num = int.Parse(ent[0]);
                    VarInt[Verify[1]] = ent[0].ToString();
                    Console.WriteLine("{0} si es un numero entero", num);
                }
                catch (FormatException)
                {
                    Console.WriteLine("{0} no es un numero entero", ent[0]);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("error de escritura");

                }
            }
            else
            {
                Console.WriteLine("Sintax Invalid.");
            }
        }


        //Metodo para crear una variable string
        public static bool CreateStringVar(string Code)
        {
            if (FindEndExpression(Code))
            {
                if (Find(Code, '='))
                {
                    string[] Sentencias = Code.Split('=');
                    string[] var = Sentencias[0].Split(' ');
                    if (process.VerifyValidate(var[1]) != TypeToken.Var)
                    {
                        Console.WriteLine("Invalid Sintax. Expression not be declared");
                        return false;
                    }
                    string[] declarate = Sentencias[1].Split(';');
                    declarate[0] = EliminateSpace(declarate[0]);
                    if (process.VerifyValidate(declarate[0]) == TypeToken.String)
                    {
                        VarString[var[1]] = declarate[0];
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Invalid Sintax. Expression not an string");
                        return false;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Sintax.Missing operator: \'=\'");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Invalid Sintax.Missing operator: \';\'");
                return false;
            }
        } 

        //Metodo para verificar ; al final
        private static bool FindEndExpression(string Code)
        {
            bool letter = false;
            for (int i = Code.Length - 1; i >= 0; i--)
            {
                if (!letter && Code[i] == ';')
                {
                    return true;
                }
                if (Code[i] != ';' && Code[i] != ' ')
                {
                    letter = true;
                    return false;
                }
            }
            return false;
        }
        
        //Metodo para encontrar una coincidencia en una linea
        public static bool Find(string Code, Char letter)
        {
            foreach(char c in Code)
            {
                if(c == letter)
                {
                    return true;
                }
            }
            return false;
        }

        //Metodo para eliminar espacios en blanco al principio de la linea
        public static string EliminateSpace(string Code)
        {
            bool inicia = false;
            string code = "";
            foreach(char c in Code)
            {
                if(!inicia && c != ' ')
                {
                    inicia = true;
                }
                if(inicia)
                {
                    code += c;
                }
            }
            return code;
        }
    }
}
