using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor.Tilemaps;
using UnityEngine;
namespace CardCompiler
{
    internal class SintaxBack
    {
        public Dictionary<string, string> VarString = new();
        public Dictionary<string, string> VarNumber = new();
        public Dictionary<string, string> VarBoolean = new();
        public List<ErrorBack> error = new();
        public List<string> TextImpress = new();
        readonly LexicalProcess process = new();

        //Funcion principal encargada de la revision de la sintaxis
        public void VerifcateSintax(string Code)
        {
            string[] Lines = Code.Split('\n'); //Separar el bloque de codigo linea a linea
            int NLine = 1;

            //Eliminar espacios al final y al principio de cada linea
            for (int i = 0; i < Lines.Length; i++) Lines[i] = EliminateSpace(Lines[i]);

            foreach (string Line in Lines)
            {
                //Verificacion de declaracion de un tipo String
                if (Regex.IsMatch(Line, @"^string|<#texto\s+[a-z](1,15)(\s+:\s+[a-z](1,15)')*;$")) CreateStringVar(Line, NLine);

                //Verificacion de declaracion de un tipo Number
                else if (Regex.IsMatch(Line, @"^number|<#real\s+[a-z](1,15)(\s+:\s+\d(0,32000))*;$")) CreateNumberVar(Line, NLine);

                //Verificacion de declaracion bool simple
                else if (Regex.IsMatch(Line, @"^bool|<#boolean\s+[a-z](1,15)(\s+:\s+(true|false))*;$")) CreateBoolVar(Line, NLine);

                //Verificacion para la creacion de cartas
                else if (Regex.IsMatch(Line, @"^card|<#definition\s+['{']*;$")) CardCreate(Lines, NLine);

                //Verificacion para la creacion de cartas
                else if (Regex.IsMatch(Line, @"^effect|<#definition\s+['{']*;$")) EffectCreate(Lines, NLine);

                //Verificacion de operador doble 
                else if (IncrementOrDecrement(Line)) continue;

                //Metodo para imprimir en pantalla
                else if (Find(Line, "Write") && Find(Line, "(") && FindEndExpression(Line, ';'))
                {
                    Write(NLine, Line);
                }

                //Verificar si hubo errores en la iteracion
                if (error.Count != 0) break;
                NLine++;
            }
        }

        //Metodo de imprimir en pantalla
        private void Write(int NLine, string Line)
        {
            string[] impresion = Line.Split('(', ')');
            impresion[1] = EliminateSpace(impresion[1]);
            if (process.VerifyValidate(impresion[1]) == TypeToken.Var)
            {
                string palabra = VarValue(VarString, impresion[1]);
                if (palabra == " ")
                {
                    palabra = VarValue(VarNumber, impresion[1]);
                }
                if (palabra != null)
                {
                    TextImpress.Add(palabra);
                }
            }
            else if (process.VerifyValidate(impresion[1]) == TypeToken.String)
            {
                TextImpress.Add(CreateString(impresion[1], NLine));
            }
            else if (process.VerifyValidate(impresion[1]) == TypeToken.Number)
            {
                TextImpress.Add(impresion[1]);
            }
            else
            {
                error.Add(new ErrorBack(NLine, ErrorValue.SintaxErrorValue));
            }
        }

        //Metodo para limpiar todo
        public void CleanSintax()
        {
            error.Clear();
            VarBoolean.Clear();
            VarNumber.Clear();
            VarString.Clear();
        }

        //Metodo para crear una variable booleana
        private void CreateBoolVar(string Line, int Nline)
        {
            if (FindEndExpression(Line, ';'))
            {
                if (Find(Line, "="))
                {
                    string[] separavar = Line.Split('=');
                    string[] Verify = separavar[0].Split(' ');
                    string[] ent = separavar[1].Split(';');
                    if (Verify.Length >= 2 && process.VerifyValidate(Verify[1]) == TypeToken.Var && Verify[0] == "bool")
                    {
                        try
                        {
                            bool var;
                            var = bool.Parse(ent[0]);
                            VarBoolean[Verify[1]] = var.ToString();
                        }
                        catch (FormatException)
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                        }
                    }
                    else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                }
                else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEqual));
            }
            else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEnd));
        }

        //Metodo para crear una variable number
        private void CreateNumberVar(string Line, int Nline)
        {
            if (FindEndExpression(Line, ';'))
            {
                if (Find(Line, "="))
                {
                    string[] separanum = Line.Split('=');
                    string[] Verify = separanum[0].Split(' ');
                    string[] ent = separanum[1].Split(';');
                    if (Verify.Length >= 2 && process.VerifyValidate(Verify[1]) == TypeToken.Var && Verify[0] == "number")
                    {
                        string num = OperationAritmetic(ent[0], Nline);
                        if (num != " ") VarNumber[Verify[1]] = num;
                    }
                    else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                }
                else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEqual));
            }
            else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEnd));
        }

        //Metodo para crear una variable string
        public bool CreateStringVar(string Code, int Nline)
        {
            if (FindEndExpression(Code, ';')) //Revisar que la declaracion de la constate termine en ;
            {
                if (Find(Code, "=")) //Encontrar el operador de asignacion de la constante
                {
                    string[] Sentencias = Code.Split('=');
                    string[] var = Sentencias[0].Split(' ');
                    if (process.VerifyValidate(var[1]) != TypeToken.Var || var[0] != "string") //Verificar que la variable sea valida del tipo string
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                        return false;
                    }
                    string[] declarate = Sentencias[1].Split(';');
                    declarate[0] = EliminateSpace(declarate[0]);

                    //Verificar si es una variable
                    if (process.VerifyValidate(declarate[0]) == TypeToken.Var) declarate[0] = VarValue(VarString, declarate[0]);

                    //Revisar si existe operacion de concatenar string
                    string notNull = CreateString(declarate[0], Nline);
                    if (notNull != " ")
                    {
                        VarString[var[1]] = notNull;
                        return true;
                    }
                    else
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                        return false;
                    }
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEqual));
                    return false;
                }
            }
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEnd));
                return false;
            }
        }

        //Metodo para verificar fnal de linea
        private bool FindEndExpression(string Code, char end)
        {
            if (Code.Length >= 1 && Code[^1] == end) return true;
            return false;
        }

        //Metodo para encontrar una coincidencia en una linea
        public bool Find(string Code, string sentence)
        {
            if (Code.Contains(sentence)) return true;
            return false;
        }

        //Metodo para eliminar espacios en blanco al principio de la linea
        public string EliminateSpace(string Code)
        {
            bool inicia = false;
            string code = "";
            int i = 0;
            int d = 0;
            for (int a = Code.Length - 1; a >= 0; a--)
            {
                if (Code[a] != ' ')
                {
                    i = a;
                    break;
                }
            }
            foreach (char c in Code)
            {
                if (!inicia && c != ' ') inicia = true;
                if (inicia && d <= i)
                {
                    code += c;
                    d++;
                }
            }
            return code;
        }

        //Metodo para crear y concatenar string usando el operador concatenar
        public string CreateString(string Code, int Nline)
        {
            Code = EliminateSpace(Code);
            if (Find(Code, "@"))
            {
                string[] var = Code.Split('@');
                for (int i = 0; i < var.Length; i++)
                {
                    var[i] = EliminateSpace(var[i]);

                    //Verificar si es una variable
                    if (process.VerifyValidate(var[i]) == TypeToken.Var) var[i] = VarValue(VarString, var[i]);
                }
                if (Find(Code, "@@"))
                {
                    int spacio = 0;
                    for (int i = 0; i < var.Length; i++)
                    {
                        if (process.VerifyValidate(var[i]) == TypeToken.String) spacio = 0;
                        if (i < var.Length - 1 && spacio < 1 && var[i] == "")
                        {
                            var[i] = "' '";
                            spacio++;
                        }
                    }
                }
                foreach (string s in var)
                {
                    if (process.VerifyValidate(s) != TypeToken.String)
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                        return " ";
                    }
                }

                string line = "";
                foreach (string s in var) foreach (char c in s) if (c != '\'') line += c;
                return line;
            }
            else if (process.VerifyValidate(Code) == TypeToken.String)
            {
                string line = "";
                foreach (char c in Code) if (c != '\'') line += c;
                return line;
            }
            else return " ";
        }

        //Metodo para encontrar un operador doble en una linea
        public bool IncrementOrDecrement(string code)

        {
            string[] num;
            if (Find(code, "++"))
            {
                num = code.Split("++");
                if (VarNumber.ContainsKey(EliminateSpace(num[0])))
                {
                    VarNumber[EliminateSpace(num[0])] = (int.Parse(VarNumber[EliminateSpace(num[0])]) + 1).ToString();
                    return true;
                }
            }
            else if (Find(code, "--"))
            {
                num = code.Split("++");
                if (VarNumber.ContainsKey(EliminateSpace(num[0])))
                {
                    VarNumber[EliminateSpace(num[0])] = (int.Parse(VarNumber[EliminateSpace(num[0])]) - 1).ToString();
                    return true;
                }
            }
            return false;
        }

        //Metodo para operaciones aritmeticas
        public string OperationAritmetic(string Code, int Nline)
        {
            string[] sum, rest, mul, div, pot;
            Code = EliminateSpace(Code);
            bool number;

            //Verificar si es un numero
            if (process.VerifyValidate(Code) == TypeToken.Number) return Code;

            //Verificar operador doble
            else if (IncrementOrDecrement(Code)) return VarValue(VarNumber, Code);

            //Verificar si es una variable
            else if (process.VerifyValidate(Code) == TypeToken.Var) return VarValue(VarNumber, Code);

            //Verificar si hay una suma
            else if (Find(Code, "+"))
            {
                number = true;
                sum = Code.Split('+');
                for (int i = 0; i < sum.Length; i++)
                {
                    sum[i] = EliminateSpace(sum[i]);
                    if (process.VerifyValidate(sum[i]) == TypeToken.Var) sum[i] = VarValue(VarNumber, sum[i]);

                    if (process.VerifyValidate(sum[i]) != TypeToken.Number) sum[i] = OperationAritmetic(sum[i], Nline);

                }
                foreach (string s in sum)
                {
                    if (process.VerifyValidate(s) != TypeToken.Number) number = false;

                }
                if (number)
                {
                    float var = float.Parse(sum[0]);
                    for (int i = 1; i < sum.Length; i++) var += float.Parse(sum[i]);
                    return var.ToString();
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                    return " ";
                }
            }

            //Verificar si hay una resta
            else if (Find(Code, "-"))
            {
                number = true;
                rest = Code.Split('-');
                for (int i = 0; i < rest.Length; i++)
                {
                    rest[i] = EliminateSpace(rest[i]);
                    if (process.VerifyValidate(rest[i]) == TypeToken.Var) rest[i] = VarValue(VarNumber, rest[i]);
                    if (process.VerifyValidate(rest[i]) != TypeToken.Number) rest[i] = OperationAritmetic(rest[i], Nline);
                }
                foreach (string s in rest) if (process.VerifyValidate(s) != TypeToken.Number) number = false;
                if (number)
                {
                    float var = float.Parse(rest[0]);
                    for (int i = 1; i < rest.Length; i++) var -= float.Parse(rest[i]);
                    return var.ToString();
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                    return " ";
                }
            }

            //Verificar si hay una multiplicacion
            else if (Find(Code, "*"))
            {
                number = true;
                mul = Code.Split('*');
                for (int i = 0; i < mul.Length; i++)
                {
                    mul[i] = EliminateSpace(mul[i]);
                    if (process.VerifyValidate(mul[i]) == TypeToken.Var) mul[i] = VarValue(VarNumber, mul[i]);
                    if (process.VerifyValidate(mul[i]) != TypeToken.Number) mul[i] = OperationAritmetic(mul[i], Nline);
                }
                foreach (string s in mul) if (process.VerifyValidate(s) != TypeToken.Number) number = false;

                if (number)
                {
                    float var = float.Parse(mul[0]);
                    for (int i = 1; i < mul.Length; i++) var *= float.Parse(mul[i]);
                    return var.ToString();
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                    return " ";
                }
            }

            //Verificar si hay division
            else if (Find(Code, "/"))
            {
                number = true;
                div = Code.Split('/');
                for (int i = 0; i < div.Length; i++)
                {
                    div[i] = EliminateSpace(div[i]);
                    if (process.VerifyValidate(div[i]) == TypeToken.Var) div[i] = VarValue(VarNumber, div[i]);
                    if (process.VerifyValidate(div[i]) != TypeToken.Number) div[i] = OperationAritmetic(div[i], Nline);
                }
                foreach (string s in div) if (process.VerifyValidate(s) != TypeToken.Number) number = false;
                if (number)
                {
                    float var = float.Parse(div[0]);
                    for (int i = 1; i < div.Length; i++) if (float.Parse(div[i]) != 0) var /= float.Parse(div[i]);
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValueZero));
                            return " ";
                        }
                    return var.ToString();
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                    return " ";
                }
            }

            //Verificar si hay potencia
            else if (Find(Code, "^"))
            {
                number = true;
                pot = Code.Split('^');
                for (int i = 0; i < pot.Length; i++)
                {
                    pot[i] = EliminateSpace(pot[i]);
                    if (process.VerifyValidate(pot[i]) == TypeToken.Var) pot[i] = VarValue(VarNumber, pot[i]);
                }
                foreach (string s in pot) if (process.VerifyValidate(s) != TypeToken.Number) number = false;
                if (number)
                {
                    for (int i = pot.Length - 2; i >= 0; i--) pot[i] = Math.Pow(double.Parse(pot[i]), double.Parse(pot[i + 1])).ToString();
                    return pot[0];
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                    return " ";
                }
            }

            //Verificar si es un error de entrada
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValue));
                return " ";
            }

        }

        //Metodo para comparaciones
        private bool Compare(string Code,int Nline)
        {
            string[] numbers;
            if(Find(Code,"<="))
            {
                numbers = Code.Split("<=");
                numbers[0] = OperationAritmetic(numbers[0],Nline);
                numbers[2] = OperationAritmetic(numbers[2], Nline);
                if (numbers[0] != " " && numbers[2] != " ")
                {
                    if (float.Parse(numbers[0]) == float.Parse(numbers[2])) return true;
                    else if (float.Parse(numbers[0]) < float.Parse(numbers[2])) return true;
                    else return false;
                }
            }
            else if(Find(Code,">="))
            {
                numbers = Code.Split("<=");
                numbers[0] = OperationAritmetic(numbers[0], Nline);
                numbers[2] = OperationAritmetic(numbers[2], Nline);
                if (numbers[0] != " " && numbers[2] != " ")
                {
                    if (float.Parse(numbers[0]) == float.Parse(numbers[2])) return true;
                    else if (float.Parse(numbers[0]) > float.Parse(numbers[2])) return true;
                    else return false;
                }
            }
            else if (Find(Code, ">"))
            {
                numbers = Code.Split('>', '=');
                numbers[0] = OperationAritmetic(numbers[0], Nline);
                numbers[2] = OperationAritmetic(numbers[2], Nline);
                if (numbers[0] != " " && numbers[2] != " ")
                {
                    if (float.Parse(numbers[0]) > float.Parse(numbers[2])) return true;
                    else return false;
                }
            }
            else if (Find(Code, "<"))
            {
                numbers = Code.Split('>', '=');
                numbers[0] = OperationAritmetic(numbers[0], Nline);
                numbers[2] = OperationAritmetic(numbers[2], Nline);
                if (numbers[0] != " " && numbers[2] != " ")
                {
                    if (float.Parse(numbers[0]) < float.Parse(numbers[2])) return true;
                    else return false;
                }
            }
            return false;
        }

        //Metodo para devolver valor de una variable tipo numero y verificar si existe
        private string VarValue(Dictionary<string, string> Variables, string Var)
        {
            foreach (var word in Variables.Keys) if (Var == word) return Variables[Var];
            return " ";
        }

        //Metodo de la definicion carta
        private void CardCreate(string[] Code, int Nline)
        {
            string Type = " ";
            string Name = " ";
            string Faction = " ";
            string Power = " ";
            string Range = " ";
            string onActivation = "*";
            if (FindEndExpression(Code[Nline - 1], '{'))
            {
                while (Code[Nline - 1] != "}")
                {
                    if (Nline + 1 < Code.Length) Nline++;
                    else break;

                    Code[Nline - 1] = EliminateSpace(Code[Nline - 1]);
                    if (Code[Nline - 1] != "")
                    {
                        string Definition = VerificateCard(Code[Nline - 1], Nline);
                        
                        if (Definition != " ")
                        {
                            string[] var = Definition.Split('-');

                            //Verificar variable Name
                            if (var[0] == "Name" && Name == " ") Name = var[1];

                            //Verificar variable Faction
                            else if (var[0] == "Faction" && Faction == " ") Faction = var[1];

                            //Verificar Variable Type
                            else if (var[0] == "Type" && Type == " ") Type = var[1];

                            //Verificar variable Power
                            else if (var[0] == "Power" && Power == " ") Power = var[1];

                            //Verificar variable Range
                            else if (var[0] == "Range" && Range == " ") for (int i = 1; i < var.Length; i++) Range += var[i] + "-";

                            //Verificar definicion OnActivation
                            else if (Definition == "OnActivation")
                            {
                                if (EliminateSpace(Code[Nline]) == "{")
                                {
                                    Nline++;
                                    onActivation = VerificateOnActivation(Code, Nline);
                                    if (onActivation != " ")
                                    {
                                        string[] on = onActivation.Split('*');
                                        int linesAdd = on[1].Split('.').Length + 9;
                                        Nline += linesAdd;
                                    }
                                }
                                else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorOpenCurlyBraces));
                            }

                            //Comprobar error
                            else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorIncorrectStatament));
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorIncorrectStatament));
                            return;
                        }
                    }
                }
                if (Code[Nline] != "}") error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorClosedCurlyBraces));

                //Crear una carta
                if (Type != " " && Name != " " && Faction != " " && Power != " " && Range != " " && error.Count == 0)
                {
                    Range = EliminateSpace(Range);
                    string card = (Name + "|" + Type + "|" + Faction + "|" + Power + "|" + Range + "|" + onActivation);
                    string decx;
                    string decks = File.ReadAllText(Application.dataPath + "/Resources/Setting/DeckInGame.txt");
                    if (File.Exists(Application.dataPath + "/Resources/Decks/" + Faction + ".txt"))
                    {
                        decx = File.ReadAllText(Application.dataPath + "/Resources/Decks/" + Faction + ".txt");
                        decx += "\n" + card;
                        SaveCard(Faction, decx, decks);
                    }
                    else SaveCard(Faction, card, decks);
                }
                else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
            }

            else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorOpenCurlyBraces));
        }

        //Metodo para verificar parametros de carta
        private string VerificateCard(string Code, int Nline)
        {
            string[] sintax;
            if (FindEndExpression(Code, ','))
            {
                if (Find(Code, ":"))
                {
                    //Verificar si es declaracion de array Range
                    if (Regex.IsMatch(Code, @"^Range|<#definition\s+['{']*;$"))
                    {
                        if (Find(Code, "[") && Find(Code, "]"))
                        {
                            string arrayAttack = "Range";
                            sintax = Code.Split('[', ']');
                            if (Find(sintax[1], ","))
                            {
                                sintax = sintax[1].Split(',');
                                if (sintax.Length > 3 || sintax.Length < 1)
                                {
                                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                                    return " ";
                                }
                                else
                                {
                                    foreach (string s in sintax)
                                    {
                                        string d;
                                        if (process.VerifyValidate(s) == TypeToken.Var) d = VarValue(VarString, s);
                                        else if (process.VerifyValidate(s) == TypeToken.String) d = CreateString(s, Nline);
                                        else
                                        {
                                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                                            return " ";
                                        }
                                        if (process.CompareAttackType(d) != AttackType.None && !Find(arrayAttack, d) && d != " ") arrayAttack += "-" + d;
                                        else
                                        {
                                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorIncorrectStatament));
                                            return " ";
                                        }
                                    }
                                    return arrayAttack;
                                }
                            }

                            else
                            {
                                if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, sintax[1]);
                                else if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = CreateString(sintax[1], Nline);
                                else
                                {
                                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                                    return " ";
                                }
                                if (process.CompareAttackType(sintax[1]) != AttackType.None && !Find(arrayAttack, sintax[1])) arrayAttack += "-" + sintax[1];
                                else
                                {
                                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorIncorrectStatament));
                                    return " ";
                                }
                            }
                            return arrayAttack;

                        }
                    }

                    //Continuar verificaciones con variables simples
                    sintax = Code.Split(':', ',');
                    sintax[1] = EliminateSpace(sintax[1]);

                    //Verificacion declaracion Name
                    if (Regex.IsMatch(Code, @"^Name|<#definition\s+['{']*;$") || Regex.IsMatch(Code, @"^Faction|<#definition\s+['{']*;$"))
                    {
                        if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, sintax[1]);
                        if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = CreateString(sintax[1], Nline);
                        if (sintax[1] != " " && Find(Code, "Name")) return "Name-" + sintax[1];
                        if (sintax[1] != " " && Find(Code, "Faction")) return "Faction-" + sintax[1];
                        else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                    }


                    //Verificacion declaracion Type
                    if (Regex.IsMatch(Code, @"^Type|<#definition\s+['{']*;$"))
                    {

                        if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, sintax[1]);
                        if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = CreateString(sintax[1], Nline);
                        if (process.CompareCardType(sintax[1]) != CardType.None) return "Type-" + sintax[1];
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                            return " ";
                        }
                    }

                    //Verificacion declaracion Power
                    if (Regex.IsMatch(Code, @"^Power|<#definition\s+['{']*;$"))
                    {
                        if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarNumber, sintax[1]);
                        if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = OperationAritmetic(sintax[1], Nline);
                        if (Find(Code, "Power") && sintax[1] != " ") return "Power-" + sintax[1];
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                            return " ";
                        }
                    }
                }
            }
            //Verificacion onActivation
            else if (FindEndExpression(Code, '['))
            {
                //Verificacion declaracion Power
                if (Regex.IsMatch(Code, @"^OnActivation|<#definition\s+['{']*;$"))
                {
                    if (Find(Code, ":"))
                    {
                        if (Code.Split(':').Length == 2) return "OnActivation";
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                            return " ";
                        }
                    }
                    else
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorAssignStatament));
                        return " ";
                    }
                }
            }
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
            }
            return " ";
        }

        //Metodo para verificar OnActivation
        private string VerificateOnActivation(string[] Code, int Nline)
        {
            bool cierre = false;
            string[] OnActive = new string[3];

            //Nombre del efecto
            if (Regex.IsMatch(Code[Nline], @"^Effect|<#definition\s+['{']*;$") && Find(Code[Nline], ":") && FindEndExpression(Code[Nline], '{'))
            {
                while (EliminateSpace(Code[Nline]) != "}" && !Regex.IsMatch(Code[Nline], @"^Selector|<#definition\s+['{']*;$"))
                {
                    Nline++;
                    //Encontrando nombres
                    if (Regex.IsMatch(Code[Nline], @"^Name|<#definition\s+['{']*;$"))
                    {
                        if (Find(Code[Nline], ":") && FindEndExpression(Code[Nline], ','))
                        {
                            string[] sintax = Code[Nline].Split(':', ',');
                            sintax[1] = EliminateSpace(sintax[1]);
                            if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, sintax[1]);

                            if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = CreateString(sintax[1], Nline);

                            if (sintax[1] != " " && File.Exists(Application.dataPath + "/Resources/Effects/" + sintax[1] + ".txt")) OnActive[0] = sintax[1];
                            
                        }
                    }

                    //Encontrando Parametros
                    if (Find(Code[Nline], ":") && Find(Code[Nline], ",") && !Regex.IsMatch(Code[Nline], @"^Name|<#definition\s+['{']*;$"))
                    {
                        string[] param = Code[Nline].Split(':', ',');
                        param[1] = EliminateSpace(param[1]);
                        param[0] = EliminateSpace(param[0]);
                        if (process.VerifyValidate(param[0]) == TypeToken.Var) OnActive[1] += param[0] + ":" + param[1] + ".";
                    }
                }
                if (EliminateSpace(Code[Nline]) == "}") cierre = true;
            }
            Nline++;

            //Selector
            if (cierre)
            {
                cierre = false;
                if (Regex.IsMatch(Code[Nline], @"^Selector|<#definition\s+['{']*;$") && Find(Code[Nline], ":") && FindEndExpression(Code[Nline], '{'))
                {
                    string source = "";
                    string single = "false";
                    string predicate = "";
                    int i = 0;
                    Nline++;
                    while (i < 3)
                    {
                        if (Regex.IsMatch(Code[Nline], @"^Source|<#definition\s+['{']*;$"))
                        {
                            if (Find(Code[Nline], ":") && FindEndExpression(Code[Nline], ','))
                            {
                                string[] sintax = Code[Nline].Split(':', ',');
                                sintax[1] = EliminateSpace(sintax[1]);
                                if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, sintax[1]);
                                if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = CreateString(sintax[1], Nline);
                                sintax[1] = process.CompareSourceType(sintax[1]);
                                if (sintax[1] != " ") source = sintax[1];
                                else error.Add(new ErrorBack(Nline+1, ErrorValue.SintaxErrorIncorrectStatament));
                            }
                        }
                        if (Regex.IsMatch(Code[Nline], @"^Single|<#definition\s+['{']*;$"))
                        {
                            if (Find(Code[Nline], ":") && FindEndExpression(Code[Nline], ','))
                            {
                                string[] sintax = Code[Nline].Split(':', ',');
                                sintax[1] = EliminateSpace(sintax[1]);
                                if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, sintax[1]);
                                sintax[1] = EliminateSpace(sintax[1]);
                                if (sintax[1] == "false" || sintax[1] == "true") single = sintax[1];
                            }
                        }
                        if (Regex.IsMatch(Code[Nline], @"^Predicate|<#definition\s+['{']*;$"))
                        {
                            if (Find(Code[Nline], ":") && FindEndExpression(Code[Nline], ','))
                            {
                                string[] sintax = Code[Nline].Split(':', ',');
                                sintax[1] = EliminateSpace(sintax[1]);
                                if (Find(sintax[1], "(") && Find(sintax[1], ")"))
                                {
                                    string[] unit = sintax[1].Split('(', ')');
                                    if (EliminateSpace(unit[1]) == "unit" && Find(sintax[1], "=>") && Find(sintax[1], "unit.")) predicate = sintax[1].Split('.')[1];
                                    else error.Add(new ErrorBack(Nline + 1, ErrorValue.SintaxErrorDeclaration));
                                }
                            }
                        }

                        i++;
                        Nline++;
                    }
                    if (source != "" && predicate != "") OnActive[2] = source + "." + single + "." + predicate;
                }
            }
            else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));

            //Creando respuesta de OnActivation
            if (EliminateSpace(Code[Nline]) == "}" && EliminateSpace(Code[Nline + 1]) == "}")
            {
                if (EliminateSpace(Code[Nline + 2]) == "]")
                {
                    if (OnActive[0] != null && OnActive[2] != null)
                    {
                        string code = OnActive[0] + "*" + OnActive[1] + "*" + OnActive[2];
                        return code;
                    }
                }
                else error.Add(new ErrorBack(Nline + 2, ErrorValue.SintaxErrorClosedBracketsBraces));
            }
            else error.Add(new ErrorBack(Nline + 1, ErrorValue.SintaxErrorClosedCurlyBraces));
            return " ";
        }

        //Metodo para guardar carta
        private void SaveCard(string Faction, string decksito, string decks)
        {
            StreamWriter sw = new(Application.dataPath + "/Resources/Decks/" + Faction + ".txt");
            if (!Find(decks, Faction))
            {
                StreamWriter sw2 = new(Application.dataPath + "/Resources/Setting/DeckInGame.txt");
                decks += "-" + Faction;
                sw2.Write(decks);
                sw2.Close();
            }
            sw.Write(decksito);
            sw.Close();
        }

        //Metodo definicion effecto
        private void EffectCreate(string[] Code, int Nline)
        {
            string Name = " ";
            Dictionary<string, string> Params = new();
            int Line = Nline - 1;

            while (Line < Code.Length)
            {
                if (Regex.IsMatch(Code[Line], @"^Name|<#definition\s+['{']*;$") && Find(Code[Line], ":") && FindEndExpression(Code[Line], ','))
                {
                    string[] sintax = Code[Line].Split(':', ',');
                    if (process.VerifyValidate(sintax[1]) == TypeToken.Var) sintax[1] = VarValue(VarString, EliminateSpace(sintax[1]));

                    if (process.VerifyValidate(sintax[1]) == TypeToken.String) sintax[1] = CreateString(EliminateSpace(sintax[1]), Nline);

                    if (sintax[1] != " ") Name = sintax[1];
                }

                if (Regex.IsMatch(Code[Line], @"^Params|<#definition\s+['{']*;$") && Find(Code[Line], ":") && FindEndExpression(Code[Line], '{'))
                {
                    Line++;
                    Nline++;
                    while (!Regex.IsMatch(Code[Line], @"^}|<#End\s+['{']*;$"))
                    {
                        string[] sintax = Code[Line].Split(':', ',');
                        Params[sintax[0]] = EliminateSpace(sintax[1]);
                        Line++;
                        Nline++;
                    }
                }

                if (Regex.IsMatch(Code[Line], @"^Action|<#definition\s+['{']*;$") && Find(Code[Line], ":") && FindEndExpression(Code[Line], '{'))
                {
                    if (Find(Code[Line], "=>"))
                    {
                        string[] sintax = Code[Line].Split(':', '=');
                        sintax = sintax[1].Split('(', ',', ')');
                        if (EliminateSpace(sintax[1]) == "targets" && EliminateSpace(sintax[2]) == "context")
                        {
                            Nline++;
                            Line++;
                            if (Name != " ") VerificateEffect(Code, Params, Name, Line);
                            else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                        }
                        else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                    }
                    else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEffectAssign));
                }
                Line++;
                Nline++;
            }

        }

        //Metodo para verificar parametros de effecto
        private void VerificateEffect(string[] Code, Dictionary<string, string> Vars, string Name, int Nline)
        {
            string[] Action = new string[Code.Length - Nline+1];
            Array.Copy(Code, Nline - 1, Action, 0, Code.Length - Nline+1);
            string Ordenes = "";
            List<string> Local_Param_Cards = new();
            List<string> Local_Param_List = new();
            List<string> Local_Param_Property = new();
            bool ActionCierre = false;
            bool EffectCierre = false;
            bool ForCierre = true;
            Local_Param_Cards.Add("");
            TextImpress.Add("revisando ordenes");

            //Eliminar espacios en blanco
            for (int i = 0; i < Action.Length; i++)
            {
                Action[i] = EliminateSpace(Action[i]);
            }

            //Revision sintactica en la declaracion de efectos
            foreach (string code in Action)
            {
                //Revision de instruccion for
                if (Regex.IsMatch(code, @"^for|<#definition\s+['{']*;$"))
                {
                    string[] forIs = code.Split(' ');
                    if (EliminateSpace(forIs[0]) == "for" && EliminateSpace(forIs[1]) == "target" && EliminateSpace(forIs[2]) == "in" && EliminateSpace(forIs[3]) == "targets" && EliminateSpace(forIs[4]) == "{")
                    {
                        Ordenes += "for-";
                        ForCierre = false;
                    }
                    else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorForUndefined));
                }

                //Revision de instruccion while
                else if (Regex.IsMatch(code, @"^while|<#definition\s+['{']*;$") && Find(code, "(") && Find(code, ")"))
                {
                    Ordenes += "while-";
                }

                //Verificar si se utiliza  o asigna una propiedad de target o context
                else if (Find(code, "."))
                {
                    //Verificar una asignacion
                    if (Find(code, "=") && FindEndExpression(code, ';'))
                    {
                        string[] assing = code.Split('=');
                        string var = EliminateSpace(assing[0]);
                        if (process.VerifyValidate(var) == TypeToken.Var)
                        {
                            assing = assing[1].Split('.', ';');

                            //Asignacion Pop
                            if (assing.Length == 4 && EliminateSpace(assing[0]) == "context")
                            {
                                if (EliminateSpace(assing[2]) == "Pop()")
                                {
                                    Local_Param_Cards.Add(var);
                                    Ordenes += "Pop|" + var + "-";
                                }

                                //Falta definir Find
                            }
                            //Asignacion de una lista
                            else if (assing.Length == 3 && VerificateContextList(EliminateSpace(assing[1])) != " ")
                            {
                                //Definir Assignacion
                                Local_Param_List.Add(var);
                                Ordenes += "ListAdd|" + var + "-";
                            }
                            //Asignacion de alguna propiedad target
                            else if (assing.Length == 3 && process.CompareTargetProperty(EliminateSpace(assing[1])) != " " && EliminateSpace(assing[0]) == "target" && Ordenes.Contains("for"))
                            {
                                //Definir Asignacion
                                Local_Param_Property.Add(var);
                                Ordenes += "PropAdd|" + var + "-";
                            }
                        }
                    }

                    //Verificar llamada a una funcion de context
                    else if ((Find(code, "(") && Find(code, ")") || Find(code, "++") || Find(code, "--")) && Find(code, ".") && Find(code, ";"))
                    {
                        string[] assign = code.Split('.', ';');
                        string function = " ";
                        string parametro = "";
                        if (assign[1].Split('(', ')').Length >= 2) parametro = (EliminateSpace(assign[1].Split('(', ')')[1]));
                        //Funciones directas del context
                        if (Local_Param_Cards.Contains(parametro) && assign.Length == 4 && EliminateSpace(assign[0]) == "context") function = ContextFunction(assign, parametro);
                        else if (assign.Length == 4 && EliminateSpace(assign[0]) == "context") function = ContextFunction(assign);
                        //Funciones de una variable que contienen una lista del context
                        else if (Local_Param_Cards.Contains(parametro) && assign.Length == 3 && Local_Param_List.Contains(EliminateSpace(assign[0]))) function = ContextFunction(assign, parametro, 1);
                        else if (assign.Length == 3 && Local_Param_List.Contains(EliminateSpace(assign[0]))) function = ContextFunction(assign, value: 1);
                        //Funciones de operador doble con un target.Power
                        if (Ordenes.Contains("for") && assign.Length == 3 && EliminateSpace(assign[0]) == "target" && EliminateSpace(assign[1]) == "Power++") function = "TargetPowerSum";
                        else if (Ordenes.Contains("for") && assign.Length == 3 && EliminateSpace(assign[0]) == "target" && EliminateSpace(assign[1]) == "Power--") function = "TargetPowerRest";
                        //Leer instrucciones
                        if (function != " ") Ordenes += function + "-";
                        else error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                    }
                }
                else if (FindEndExpression(code, '}') && Ordenes.Contains("for") && !ActionCierre && !EffectCierre && !ForCierre)
                {
                    ForCierre = true;
                }
                else if (FindEndExpression(code, '}') && !ActionCierre && ForCierre)
                {
                    ActionCierre = true;
                }
                else if(FindEndExpression(code, '}') && ActionCierre && ForCierre && !EffectCierre)
                {
                    EffectCierre = true;
                }
                Nline++;
            }

            string vars = "";
            foreach (string s in Vars.Keys)
            {
                vars += s + "|" + Vars[s] + "^";
            }
            TextImpress.Add(Ordenes);
            if(Ordenes != " " && ActionCierre && EffectCierre && ForCierre)
            {
                string save = CreateString(Name,Nline) + "&" + vars + "&" + Ordenes;
                StreamWriter sw = new(Application.dataPath + "/Resources/Effects/" + CreateString(Name,Nline) + ".txt");
                sw.Write(save);
                sw.Close();
            }
            else error.Add(new ErrorBack(Nline,ErrorValue.SintaxErrorDeclaration));
        }

        //Metodo para verificar listar existentes del context en una linea
        public string VerificateContextList(string Code)
        {
            Code = EliminateSpace(Code);
            if (Code == "TriggerPlayer") return Code;
            if (Code == "Hand") return Code;
            if (Code == "Deck") return Code;
            if (Code == "Graveyard") return Code;
            if (Code == "Field") return Code;
            if (Code == "Board") return Code;
            if (Find(Code, "(") && Find(Code, ")"))
            {
                string[] ofPlayer = Code.Split('(', ')');
                if (Find(EliminateSpace(ofPlayer[0]), "DeckOfPlayer"))
                {
                    if (EliminateSpace(ofPlayer[1]) == "context.TriggerPlayer") return "Deck";
                    else if (process.VerifyValidate(EliminateSpace(ofPlayer[1])) == TypeToken.Var) return ofPlayer[1];
                }
                if (Find(EliminateSpace(ofPlayer[0]), "GraveyardOfPlayer"))
                {
                    if (EliminateSpace(ofPlayer[1]) == "context.TriggerPlayer") return "Graveyard";
                    else if (process.VerifyValidate(EliminateSpace(ofPlayer[1])) == TypeToken.Var) return ofPlayer[1];
                }
                if (Find(EliminateSpace(ofPlayer[0]), "HandOfPlayer"))
                {
                    if (EliminateSpace(ofPlayer[1]) == "context.TriggerPlayer") return "Hand";
                    else if (process.VerifyValidate(EliminateSpace(ofPlayer[1])) == TypeToken.Var) return ofPlayer[1];
                }
                if (Find(EliminateSpace(ofPlayer[0]), "FieldOfPlayer"))
                {
                    if (EliminateSpace(ofPlayer[1]) == "context.TriggerPlayer") return "Field";
                    else if (process.VerifyValidate(EliminateSpace(ofPlayer[1])) == TypeToken.Var) return ofPlayer[1];
                }
            }
            return " ";
        }

        //Determinar funcion del context
        private string ContextFunction(string[] var, string param = " ", int value = 2)
        {
            if (EliminateSpace(var[value]) == "Pop()") return "Pop";
            if (EliminateSpace(var[value]) == "Remove(" + param + ")") return "Remove|" + param;
            if (EliminateSpace(var[value]) == "Push(" + param + ")") return "Push|" + param; ;
            if (EliminateSpace(var[value]) == "Add(" + param + ")") return "Add|" + param;
            if (EliminateSpace(var[value]) == "SendBottom(" + param + ")") return "SendBottom|" + param;
            if (EliminateSpace(var[value]) == "Shufle()") return "Shuffle";

            return " ";
        }
    }
}
