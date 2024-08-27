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
        LexicalProcess process = new();

        //Funcion principal encargada de la revision de la sintaxis
        public void VerifcateSintax(string Code)
        {
            string[] Lines = Code.Split('\n'); //Separar el bloque de codigo linea a linea
            int NLine = 1;

            //Eliminar espacios al final y al principio de cada linea
            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i] = EliminateSpace(Lines[i]);

            }

            foreach (string Line in Lines)
            {
                //Verificacion de declaracion de un tipo String
                if (Regex.IsMatch(Line, @"^string|<#texto\s+[a-z](1,15)(\s+:\s+[a-z](1,15)')*;$"))
                {
                    CreateStringVar(Line, NLine);
                }

                //Verificacion de declaracion de un tipo Number
                else if (Regex.IsMatch(Line, @"^number|<#real\s+[a-z](1,15)(\s+:\s+\d(0,32000))*;$"))
                {
                    CreateNumberVar(Line, NLine);
                }

                //Verificacion de declaracion bool simple
                if (Regex.IsMatch(Line, @"^bool|<#boolean\s+[a-z](1,15)(\s+:\s+(true|false))*;$"))
                {
                    CreateBoolVar(Line, NLine);
                }

                //Verificacion para la creacion de cartas
                if (Regex.IsMatch(Line, @"^card|<#definition\s+['{']*;$"))
                {
                    CardCreate(Lines, NLine);
                }

                //Verificacion para la creacion de cartas
                if (Regex.IsMatch(Line, @"^effect|<#definition\s+['{']*;$"))
                {
                    EffectCreate(Lines, NLine);
                }

                //Metodo para imprimir en pantalla por aburrimiento
                if (Find(Line, "Write") && Find(Line, "(") && FindEndExpression(Line, ';'))
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

                //Verificar si hubo errores en la iteracion
                if (error.Count != 0)
                {
                    break;
                }
                NLine++;
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
                    else
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                    }
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEqual));
                }
            }
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEnd));
            }

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
                        if (num != " ")
                        {
                            VarNumber[Verify[1]] = num;
                        }

                    }

                    else
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
                    }
                }
                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEqual));
                }
            }
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEnd));
            }
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
                    if (process.VerifyValidate(declarate[0]) == TypeToken.Var)
                    {
                        declarate[0] = VarValue(VarString, declarate[0]);
                    }

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

        //Metodo para verificar ; al final
        private bool FindEndExpression(string Code, char end)
        {
            if (Code[^1] == end)
            {
                return true;
            }
            return false;
        }

        //Metodo para encontrar una coincidencia en una linea
        public bool Find(string Code, string sentence)
        {
            if (Code.Contains(sentence))
            {
                return true;
            }
            return false;
        }

        //Metodo para encontrar un operador doble en una linea

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
                if (!inicia && c != ' ')
                {
                    inicia = true;
                }
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
                    if (process.VerifyValidate(var[i]) == TypeToken.Var)
                    {
                        var[i] = VarValue(VarString, var[i]);
                    }
                }
                if (Find(Code, "@@"))
                {
                    int spacio = 0;
                    for (int i = 0; i < var.Length; i++)
                    {
                        if (process.VerifyValidate(var[i]) == TypeToken.String)
                        {
                            spacio = 0;
                        }
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
                foreach (string s in var)
                {
                    foreach (char c in s)
                    {
                        if (c != '\'')
                        {
                            line += c;
                        }
                    }

                }
                return line;

            }
            else if (process.VerifyValidate(Code) == TypeToken.String)
            {
                string line = "";
                foreach (char c in Code)
                {
                    if (c != '\'')
                    {
                        line += c;
                    }
                }
                return line;
            }
            else
            {
                return " ";
            }
        }

        //Metodo para operaciones aritmeticas
        public string OperationAritmetic(string Code, int Nline)
        {
            string[] sum, rest, mul, div, pot;
            Code = EliminateSpace(Code);
            bool number;

            //Verificar si es un numero
            if (process.VerifyValidate(Code) == TypeToken.Number)
            {
                return Code;
            }

            //Verificar si es una variable
            else if (process.VerifyValidate(Code) == TypeToken.Var)
            {
                return VarValue(VarNumber, Code);
            }
            //Verificar si hay una suma
            else if (Find(Code, "+"))
            {
                number = true;
                sum = Code.Split('+');
                for (int i = 0; i < sum.Length; i++)
                {
                    sum[i] = EliminateSpace(sum[i]);
                    if (process.VerifyValidate(sum[i]) == TypeToken.Var)
                    {
                        sum[i] = VarValue(VarNumber, sum[i]);
                    }
                    if (process.VerifyValidate(sum[i]) != TypeToken.Number)
                    {
                        sum[i] = OperationAritmetic(sum[i], Nline);
                    }
                }
                foreach (string s in sum)
                {
                    if (process.VerifyValidate(s) != TypeToken.Number)
                    {
                        number = false;
                    }
                }
                if (number)
                {
                    float var = float.Parse(sum[0]);
                    for (int i = 1; i < sum.Length; i++)
                    {
                        var += float.Parse(sum[i]);
                    }
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
                    if (process.VerifyValidate(rest[i]) == TypeToken.Var)
                    {
                        rest[i] = VarValue(VarNumber, rest[i]);
                    }
                    if (process.VerifyValidate(rest[i]) != TypeToken.Number)
                    {
                        rest[i] = OperationAritmetic(rest[i], Nline);
                    }
                }
                foreach (string s in rest)
                {
                    if (process.VerifyValidate(s) != TypeToken.Number)
                    {
                        number = false;
                    }
                }
                if (number)
                {
                    float var = float.Parse(rest[0]);
                    for (int i = 1; i < rest.Length; i++)
                    {
                        var -= float.Parse(rest[i]);
                    }
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
                    if (process.VerifyValidate(mul[i]) == TypeToken.Var)
                    {
                        mul[i] = VarValue(VarNumber, mul[i]);
                    }
                    if (process.VerifyValidate(mul[i]) != TypeToken.Number)
                    {
                        mul[i] = OperationAritmetic(mul[i], Nline);
                    }
                }
                foreach (string s in mul)
                {
                    if (process.VerifyValidate(s) != TypeToken.Number)
                    {
                        number = false;
                    }
                }
                if (number)
                {
                    float var = float.Parse(mul[0]);
                    for (int i = 1; i < mul.Length; i++)
                    {
                        var *= float.Parse(mul[i]);
                    }
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
                    if (process.VerifyValidate(div[i]) == TypeToken.Var)
                    {
                        div[i] = VarValue(VarNumber, div[i]);
                    }
                    if (process.VerifyValidate(div[i]) != TypeToken.Number)
                    {
                        div[i] = OperationAritmetic(div[i], Nline);
                    }
                }
                foreach (string s in div)
                {
                    if (process.VerifyValidate(s) != TypeToken.Number)
                    {
                        number = false;
                    }
                }
                if (number)
                {
                    float var = float.Parse(div[0]);
                    for (int i = 1; i < div.Length; i++)
                    {
                        if (float.Parse(div[i]) != 0)
                        {
                            var /= float.Parse(div[i]);
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorValueZero));
                            return " ";
                        }

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
                    if (process.VerifyValidate(pot[i]) == TypeToken.Var)
                    {
                        pot[i] = VarValue(VarNumber, pot[i]);
                    }
                }
                foreach (string s in pot)
                {
                    if (process.VerifyValidate(s) != TypeToken.Number)
                    {
                        number = false;
                    }
                }
                if (number)
                {
                    for (int i = pot.Length - 2; i >= 0; i--)
                    {
                        pot[i] = Math.Pow(double.Parse(pot[i]), double.Parse(pot[i + 1])).ToString();
                    }

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

        //Metodo para devolver valor de una variable tipo numero y verificar si existe
        private string VarValue(Dictionary<string, string> Variables, string Var)
        {
            foreach (var word in Variables.Keys)
            {
                if (Var == word)
                {
                    return Variables[Var];
                }
            }
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
            int EndLine = Nline + 5;

            if ((EndLine <= Code.Length))
            {
                if (Find(Code[Nline - 1], "{") && FindEndExpression(Code[Nline - 1], '{'))
                {
                    EndLine--;
                    while (Nline < Code.Length - 1)
                    {
                        Nline++;
                        Code[Nline - 1] = EliminateSpace(Code[Nline - 1]);
                        string Definition = VerificateCard(Code[Nline - 1], Nline);

                        if (Definition != " ")
                        {
                            string[] var = Definition.Split('-');
                            
                            //Verificar variable Name
                            if (var[0] == "Name" && Name == " ")
                            {
                                Name = var[1];
                            }
                            
                            //Verificar variable Faction
                            else if (var[0] == "Faction" && Faction == " ")
                            {
                                Faction = var[1];
                            }

                            //Verificar Variable Type
                            else if (var[0] == "Type" && Type == " ")
                            {
                                Type = var[1];
                            }

                            //Verificar variable Power
                            else if (var[0] == "Power" && Power == " ")
                            {
                                Power = var[1];
                            }

                            //Verificar variable Range
                            else if (var[0] == "Range" && Range == " ")
                            {
                                for (int i = 1; i < var.Length; i++)
                                {
                                    Range += var[i] + "-";
                                }
                            }

                            else if (Definition == "OnActivation")
                            {
                                string onActivation = VerificateOnActivation(Code,Nline);
                                if(onActivation != " ")
                                {
                                    EndLine += onActivation.Split('*').Length;
                                }
                            }

                            //Comprobar error
                            else
                            {
                                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorIncorrectStatament));
                            }

                            EndLine--;

                            if (EndLine == 0)
                            {
                                break;
                            }
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorIncorrectStatament));
                            return;
                        }
                    }
                    if (Code[Nline] != "}")
                    {
                        error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorClosedCurlyBraces));
                    }

                    if (Type != " " && Name != " " && Faction != " " && Power != " " && Range != " ")
                    {
                        //Crea una carta
                        Range = EliminateSpace(Range);
                        string card = (Name + "|" + Type + "|" + Faction + "|" + Power + "|" + Range);
                        string decksito;
                        string decks = File.ReadAllText(Application.dataPath + "/Resources/Setting/DeckInGame.txt");
                        if (File.Exists(Application.dataPath + "/Resources/Decks/" + Faction + ".txt"))
                        {
                            decksito = File.ReadAllText(Application.dataPath + "/Resources/Decks/" + Faction + ".txt");
                            decksito += ";" + card;
                            SaveCard(Faction, decksito, decks);
                        }
                        else
                        {
                            SaveCard(Faction, card, decks);
                        }
                    }
                }

                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorOpenCurlyBraces));
                }
            }
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
            }
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
                                        if (process.VerifyValidate(s) == TypeToken.Var)
                                        {
                                            d = VarValue(VarString, s);
                                        }
                                        else if (process.VerifyValidate(s) == TypeToken.String)
                                        {
                                            d = CreateString(s, Nline);
                                        }
                                        else
                                        {
                                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                                            return " ";
                                        }
                                        if (process.CompareAttackType(d) != AttackType.None && !Find(arrayAttack, d) && d != " ")
                                        {

                                            arrayAttack += "-" + d;
                                        }
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
                                if (process.VerifyValidate(sintax[1]) == TypeToken.Var)
                                {
                                    sintax[1] = VarValue(VarString, sintax[1]);
                                }
                                else if (process.VerifyValidate(sintax[1]) == TypeToken.String)
                                {
                                    sintax[1] = CreateString(sintax[1], Nline);
                                }
                                else
                                {
                                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                                    return " ";
                                }
                                if (process.CompareAttackType(sintax[1]) != AttackType.None && !Find(arrayAttack, sintax[1]))
                                {

                                    arrayAttack += "-" + sintax[1];
                                }
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
                        if (process.VerifyValidate(sintax[1]) == TypeToken.Var)
                        {
                            sintax[1] = VarValue(VarString, sintax[1]);
                        }

                        if (process.VerifyValidate(sintax[1]) == TypeToken.String)
                        {
                            sintax[1] = CreateString(sintax[1], 0);
                        }

                        if (sintax[1] != " " && Find(Code, "Name"))
                        {
                            return "Name-" + sintax[1];
                        }
                        if (sintax[1] != " " && Find(Code, "Faction"))
                        {
                            return "Faction-" + sintax[1];
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                        }
                    }


                    //Verificacion declaracion Type
                    if (Regex.IsMatch(Code, @"^Type|<#definition\s+['{']*;$"))
                    {

                        if (process.VerifyValidate(sintax[1]) == TypeToken.Var)
                        {
                            sintax[1] = VarValue(VarString, sintax[1]);
                        }
                        if (process.VerifyValidate(sintax[1]) == TypeToken.String)
                        {
                            sintax[1] = CreateString(sintax[1], 0);
                        }
                        if (process.CompareCardType(sintax[1]) != CardType.None)
                        {
                            return "Type-" + sintax[1];
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                            return " ";
                        }
                    }

                    //Verificacion declaracion Power
                    if (Regex.IsMatch(Code, @"^Power|<#definition\s+['{']*;$"))
                    {
                        if (process.VerifyValidate(sintax[1]) == TypeToken.Var)
                        {
                            sintax[1] = VarValue(VarNumber, sintax[1]);
                        }
                        if (process.VerifyValidate(sintax[1]) == TypeToken.String)
                        {
                            sintax[1] = OperationAritmetic(sintax[1], Nline);
                        }
                        if (Find(Code, "Power") && sintax[1] != " ")
                        {
                            return "Power-" + sintax[1];
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                            return " ";
                        }
                    }

                    //Verificacion declaracion Power
                    if (Regex.IsMatch(Code, @"^OnActivation|<#definition\s+['{']*;$") && FindEndExpression(Code,'['))
                    {
                        if(Find(Code,":"))
                        {
                            if(Code.Split(':').Length == 3)
                            {
                                return "OnActivation";
                            }
                            else
                            {
                                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                                return " ";
                            }
                        }
                        else
                        {
                            error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorUndefined));
                            return " ";
                        }
                    }
                }

                else
                {
                    error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorAssignStatament));
                    return " ";
                }
            }

            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorEndStatament));
                return " ";
            }
            return " ";
        }

        //Metodo para verificar OnActivation
        private string VerificateOnActivation(string[] Code, int Nline)
        {
            return " ";
        }

        //Metodo para guardar carta
        private void SaveCard(string Faction, string decksito, string decks)
        {
            StreamWriter sw = new(Application.dataPath + "/Resources/Decks/" + Faction + ".txt");

            StreamWriter sw2 = new(Application.dataPath + "/Resources/Setting/DeckInGame.txt");
            if (!Find(decks, Faction))
            {
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
            string Action = "";
            int Line = Nline - 1;

            while (Line < Code.Length)
            {
                if (Regex.IsMatch(Code[Line], @"^Name|<#definition\s+['{']*;$") && Find(Code[Line], ":") && FindEndExpression(Code[Line], ','))
                {
                    string[] sintax = Code[Line].Split(':', ',');
                    if (process.VerifyValidate(sintax[1]) == TypeToken.Var)
                    {
                        sintax[1] = VarValue(VarString, sintax[1]);
                    }

                    if (process.VerifyValidate(sintax[1]) == TypeToken.String)
                    {
                        sintax[1] = CreateString(sintax[1], 0);
                    }

                    if (sintax[1] != " ")
                    {
                        Name = sintax[1];
                    }
                }

                if (Regex.IsMatch(Code[Line], @"^Params|<#definition\s+['{']*;$") && Find(Code[Line], ":") && FindEndExpression(Code[Line], '{'))
                {
                    Line++;
                    Nline++;
                    while (!Regex.IsMatch(Code[Line], @"^}|<#End\s+['{']*;$"))
                    {
                        string[] sintax = Code[Line].Split(":", ',');
                        Params[sintax[0]] = sintax[1];
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
                            while (!Regex.IsMatch(Code[Line], @"^}|<#End\s+['{']*;$"))
                            {
                                Action += Code[Line] + "-";
                                Line++;
                                Nline++;
                            }
                        }
                    }
                }
                Line++;
                Nline++;
            }
            if (Name != " " && Action != " ")
            {
                VerificateEffect(Action, Params, Name, Nline);
            }
            else
            {
                error.Add(new ErrorBack(Nline, ErrorValue.SintaxErrorDeclaration));
            }

        }

        //Metodo para verificar parametros de effecto
        private string VerificateEffect(string Code, Dictionary<string, string> Vars, string Name, int Nline)
        {
            string[] Action = Code.Split('-');
            string Ordenes = "";
            List<string> Local_Param_Cards = new();
            List<string> Local_Param_List = new();

            Local_Param_Cards.Add("");
            TextImpress.Add("revisando ordenes");
            foreach (string code in Action)
            {
                string[] Line = code.Split(' ');
                for (int i = 0; i < Line.Length; i++)
                {
                    Line[i] = EliminateSpace(Line[i]);
                }
                if (Line[0] == "for" && Line[1] == "target" && Line[2] == "in" && Line[3] == "targets" && Line[4] == "{")
                {
                    TextImpress.Add("Es un for");
                }

                //Si es asignar
                else if (Line.Length == 3 && Line[1] == "=" && FindEndExpression(Line[2], ';'))
                {
                    if (process.VerifyValidate(Line[0]) == TypeToken.Var)
                    {
                        string[] var = Line[2].Split(';');
                        if (Find(var[0], "."))
                        {
                            var = var[0].Split('.');
                            if (EliminateSpace(var[0]) == "context")
                            {
                                //si es asignar propia
                                if (EliminateSpace(var[1]) == "Hand" || EliminateSpace(var[1]) == "Deck" || EliminateSpace(var[1]) == "Field" || EliminateSpace(var[1]) == "Graveyard")
                                {
                                    if (ContextFunction(var) == "Pop")
                                    {
                                        Ordenes += "Pop|" + Line[0] + "|" + var[1] + "-";
                                        Local_Param_Cards.Add(Line[0]);
                                    }
                                }
                                else if (ContextFunction(var, value: 1) != " ")
                                {

                                }
                            }
                            if (EliminateSpace(var[0]) == "target")
                            {
                                if (EliminateSpace(var[1]) == "Owner")
                                {
                                    TextImpress.Add("Es una asignacion propia");
                                    Local_Param_Cards.Add(Line[0]);
                                }
                                if (EliminateSpace(var[1]) == "Power")
                                {
                                    TextImpress.Add("Es una asignacion del poder");
                                    Local_Param_Cards.Add(Line[0]);
                                }
                            }

                        }
                        
                    }
                }
                //si no es asignar
                else if (Find(Line[0],"."))
                {
                    string[] var = Line[0].Split('.', ';');

                    if (EliminateSpace(var[0]) == "context" || Local_Param_List.Contains(EliminateSpace(var[0])))
                    {
                        //Si es propia
                        if (EliminateSpace(var[1]) == "Hand" || EliminateSpace(var[1]) == "Deck" || EliminateSpace(var[1]) == "Field" || EliminateSpace(var[1]) == "Graveyard")
                        {
                            foreach (string par in Local_Param_Cards)
                            {
                                string fun = ContextFunction(var, par);
                                if (fun == "Shuffle")
                                {
                                    Ordenes += "Shufle" + "|" + var[1].ToString() + "-";
                                    break;
                                }
                               
                                if (fun != " " && fun == "Push")
                                {
                                    Ordenes += "Push" + "|" + var[1] + "|" + par + "-";
                                    break;
                                }
                                if (fun != " " && fun == "Add")
                                {
                                    Ordenes += "Push" + "|" + var[1] + "|" + par + "-";
                                    break;
                                }
                                if (fun != " " && fun == "Remove")
                                {
                                    Ordenes += "Remove" + "|" + var[1] + "|" + par + "-";
                                    break;
                                }
                            }
                        }
                        //Si es de una variable
                        else
                        {
                            foreach (string par in Local_Param_List)
                            {
                                string fun = ContextFunction(var, par,1);
                                if (fun == "Shuffle")
                                {
                                    Ordenes += "Shufle" + "|" + var[1].ToString() + "-";
                                    break;
                                }

                                if (fun != " " && fun == "Push")
                                {
                                    Ordenes += "Push" + "|" + var[1] + "|" + par + "-";
                                    break;
                                }
                                if (fun != " " && fun == "Add")
                                {
                                    Ordenes += "Push" + "|" + var[1] + "|" + par + "-";
                                    break;
                                }
                                if (fun != " " && fun == "Remove")
                                {
                                    Ordenes += "Remove" + "|" + var[1] + "|" + par + "-";
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            string param = "";
            foreach(string par in Local_Param_Cards)
            {
                param += par + "-";
            }
            StreamWriter sw = new(Application.dataPath + "/Resources/Effects/" + CreateString(Name,Nline) + ".txt");
            sw.Write(CreateString(Name,Nline) + "*" + Ordenes + "*" + param);
            sw.Close();
            return " ";
        }

        //Determinar funcion del context
        private string ContextFunction(string[] var, string param = " ", int value = 2)
        {
            if (EliminateSpace(var[value]) == "Pop()")
            {
                TextImpress.Add("Es un Pop");
                return "Pop";
            }
            if (EliminateSpace(var[value]) == "Remove(" + param + ")")
            {
                TextImpress.Add("Es un Remove");
                return "Remove";
            }
            if (EliminateSpace(var[value]) == "Push(" + param + ")")
            {
                TextImpress.Add("Es un Push");
                return "Push";
            }
            if (EliminateSpace(var[value]) == "Add(" + param + ")")
            {
                TextImpress.Add("Es un Add");
                return "Push";
            }
            if (EliminateSpace(var[value]) == "SendBottom(" + param + ")")
            {
                TextImpress.Add("Es un SendBottom");
                return "SendBottom";
            }
            if (EliminateSpace(var[value]) == "Shufle()")
            {
                TextImpress.Add("Es un Shufle");
                return "Shuffle";
            }

            return " ";
        }

    } 
}
