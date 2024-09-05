using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;

namespace CardCompiler
{
    public class effect
    {
        public string Name;
        public string[] Source;
        public Card card;
        public List<string> Instruntions;
        public List<string> Local_param;
        public List<string> Targets;
        public Dictionary<string, string> Params;
        readonly SintaxBack process = new();
        readonly LexicalProcess Revision = new();
        int instruccion = 0;

        //Parametros locales
        Dictionary<string, GeneralCard> Local_Param_Cards = new();

        public effect(string Definicion, string source, Card card, string Param)
        {
            string[] parts = Definicion.Split('&');
            Name = process.EliminateSpace(parts[0]);
            Local_param = parts[1].Split('^').ToList();
            Instruntions = parts[2].Split('-').ToList();
            Source = source.Split('.');
            if (Param != "")
            {
                string[] parametros = Param.Split('.');
                foreach (string s in parametros)
                {
                    if (s.Contains(":"))
                    {
                        Params[s.Split(':')[0]] = s.Split(':')[1];
                        if (Revision.VerifyValidate(process.EliminateSpace(s.Split(':')[1])) == TypeToken.Number)
                        {
                            string value = "number " + process.EliminateSpace(s.Split(':')[0] + " = " + process.EliminateSpace(s.Split(':')[1]) + ";");
                            process.CreateNumberVar(value, 0);
                        }
                        if (Revision.VerifyValidate("'" + process.EliminateSpace(s.Split(':')[1]) + "'") == TypeToken.String)
                        {
                            string value = "string " + process.EliminateSpace(s.Split(':')[0] + " = " + process.EliminateSpace(s.Split(':')[1]) + ";");
                            process.CreateNumberVar(value, 0);
                        }
                    }
                }
            }
            this.card = card;
        }

        public void Action()
        {
            bool repetir = true;
            string whi = "";
            int whileintru = 0;
            while (repetir)
            {
                for (int i = instruccion; i < Instruntions.Count; i++)
                {
                    string instrucion = Instruntions[i];
                    if (instrucion != "")
                    {
                        string[] parts = instrucion.Split('|');
                        if (parts[0] == "while")
                        {
                            whi = parts[1];
                            whileintru = i+1;
                        }
                        if (parts.Length == 3) DeterminateList(parts[1],GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context, parts[0], parts[2]);
                        else DeterminateList(parts[1], GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context, parts[0]);
                        i = instruccion;
                        instruccion++;
                    }
                }
                if (process.Compare(whi, 0))
                {
                    repetir = true;
                    instruccion = whileintru;
                }
                else repetir = false;
            }
        }
        public List<GeneralCard> EjecutarEfecto(List<GeneralCard> list, string instruction, string var = " ")
        {
            GeneralCard card;
            switch (instruction)
            {
                case "Pop":
                    card = list[0];
                    Local_Param_Cards[var] = card;
                    list.RemoveAt(0);
                    return list;

                case "Add":
                    if (Local_Param_Cards.ContainsKey(var))
                    {
                        list.Add(Local_Param_Cards[var]);
                        Local_Param_Cards.Remove(var);
                    }
                    return list;

                case "Shufle":
                    for (int i = 0; i < list.Count; i++)
                    {
                        int d = UnityEngine.Random.Range(0, list.Count);
                        card = list[i];
                        list[i] = list[d];
                        list[d] = card;
                    }
                    return list;

                case "Push":
                    if (Local_Param_Cards.ContainsKey(var))
                    {
                        list[0] = Local_Param_Cards[var];
                        Local_Param_Cards.Remove(var);
                    }
                    return list;

                case "SendBottom":
                    if (Local_Param_Cards.ContainsKey(var))
                    {
                        list[list.Count - 1] = Local_Param_Cards[var];
                        Local_Param_Cards.Remove(var);
                    }
                    return list;

                case "Remove":
                    if (Local_Param_Cards.ContainsKey(var))
                    {
                        list.Remove(Local_Param_Cards[var]);
                        Local_Param_Cards.Remove(var);
                    }
                    return list;

                case "Find":
                    string[] parts;
                    if (Instruntions[instruccion].Contains("=="))
                    {
                        parts = Instruntions[instruccion].Split("==");
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (process.EliminateSpace(parts[0].Split("|")[3]) == "Type" && list[i].Type.ToString() == process.CreateString(process.EliminateSpace(parts[1]),0))
                            {
                                Local_Param_Cards[parts[0].Split("|")[2]] = list[i];
                                break;
                            }
                            if (process.EliminateSpace(parts[0].Split("|")[3]) == "Faction" && list[i].Faction == process.CreateString(process.EliminateSpace(parts[1]),0))
                            {
                                Local_Param_Cards[parts[0].Split("|")[2]] = list[i];
                                break;
                            }
                            if (process.EliminateSpace(parts[0].Split("|")[3]) == "Power" && list[i].Power.ToString() == process.EliminateSpace(parts[1]))
                            {
                                Local_Param_Cards[parts[0].Split("|")[2]] = list[i];
                                break;
                            }
                        }
                    }                 
                    return list;

                case "NumVar":
                    string[] N = Instruntions[instruccion].Split('|');
                    process.CreateNumberVar(N[2], 0);
                    return list;

                case "StrVar":
                    string[] S = Instruntions[instruccion].Split('|');
                    process.CreateStringVar(S[2], 0);
                    return list;

                case "BoolVar":
                    string[] B = Instruntions[instruccion].Split('|');
                    process.CreateBoolVar(B[2], 0);
                    return list;

                case "for":
                    instruccion++;
                    int s = instruccion;
                    for (int i = 0; i < list.Count; i++)
                    {
                        instruccion = s;
                        UnityEngine.Debug.Log(list.Count);
                        while (instruccion < Instruntions.Count)
                        {
                            string[] W = Instruntions[instruccion].Split('|');
                            if (W[0].Contains("while"))
                            {
                                while (process.Compare(W[1], 0))
                                {
                                    instruccion++;
                                    if (Instruntions[instruccion].Contains("Target"))
                                    {
                                        list[i] = TargetFunction(list[i], Instruntions[instruccion].Split('|')[0], Instruntions[instruccion].Split('|')[2]);
                                    }
                                    else
                                    {
                                        string instrucion = Instruntions[instruccion];
                                        if (instrucion != "")
                                        {
                                            string[] part = instrucion.Split('|');                                           
                                            if (part.Length == 3) DeterminateList(part[1], GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context, part[0], part[2]);
                                            else DeterminateList(part[1], GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context, part[0]);
                                        }
                                    }
                                }
                                if (Source[1] == "true") break;
                            }
                            else
                            {
                                if (Instruntions[instruccion].Contains("Target"))
                                {
                                    list[i] = TargetFunction(list[i], Instruntions[instruccion].Split('|')[0], Instruntions[instruccion].Split('|')[2]);
                                }
                                else
                                {
                                    string instrucion = Instruntions[instruccion];
                                    if (instrucion != "")
                                    {
                                        string[] part = instrucion.Split('|');
                                        if (part.Length == 3) DeterminateList(part[1], GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context, part[0], part[2]);
                                        else DeterminateList(part[1], GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context, part[0]);
                                    }
                                }
                                if (Source[1] == "true") break;
                            }
                            instruccion++;
                        }
                    }
                    return list;
            }
            return list;

            GeneralCard TargetFunction(GeneralCard Card, string action, string value = " ")
            {
                if (Source[2].Contains("==") && (Card.Type == CardType.Plata || Card.Type == CardType.Oro))
                {
                    string[] parts = Source[2].Split("==");
                    if (process.EliminateSpace(parts[0]) == "Name" && Card.Name.ToString() == process.CreateString(process.EliminateSpace(parts[1]), 0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                    if (process.EliminateSpace(parts[0]) == "Type" && Card.Type.ToString() == process.CreateString(process.EliminateSpace(parts[1]), 0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                    if (process.EliminateSpace(parts[0]) == "Faction" && Card.Faction == process.CreateString(process.EliminateSpace(parts[1]), 0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                    if (process.EliminateSpace(parts[0]) == "Power" && Card.Power.ToString() == process.OperationAritmetic(parts[1],0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                }
                else if (Source[2].Contains("<=") && (Card.Type == CardType.Plata || Card.Type == CardType.Oro))
                {
                    string[] parts = Source[2].Split("<=");
                    if (process.EliminateSpace(parts[0]) == "Power" && process.Compare(Card.Power.ToString() + " <= " + parts[1],0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                }
                else if (Source[2].Contains("<") && (Card.Type == CardType.Plata || Card.Type == CardType.Oro) )
                {
                    string[] parts = Source[2].Split("<");
                    if (process.EliminateSpace(parts[0]) == "Power" && process.Compare(Card.Power.ToString() + "<" + parts[1],0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                }
                else if (Source[2].Contains(">=") && (Card.Type == CardType.Plata || Card.Type == CardType.Oro))
                {
                    string[] parts = Source[2].Split(">=");
                    if (process.EliminateSpace(parts[0]) == "Power" && process.Compare(Card.Power.ToString() + ">=" +parts[1],0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                }
                else if (Source[2].Contains(">") && (Card.Type == CardType.Plata || Card.Type == CardType.Oro))
                {
                    string[] parts = Source[2].Split(">");
                    if (process.EliminateSpace(parts[0]) == "Power" && process.Compare(Card.Power.ToString() + ">" + parts[1],0))
                    {
                        Card = PowerChance(Card, action, value);
                    }
                }

                return Card;
            }
            GeneralCard PowerChance(GeneralCard card,string action,string value = " ")
            {
                process.VarNumber["Temp"] = card.Power.ToString();
                string newPower = " ";
                if(action == "TargetPowerSum")
                {
                    newPower = process.OperationAritmetic("Temp++",0);
                }
                if(action == "TargetPowerRest")
                {
                    newPower = process.OperationAritmetic("Temp--", 0);
                }
                if(action == "TargetPowerRestTo")
                {
                    newPower = process.OperationAritmetic("Temp-=" + value, 0);
                }
                if(action == "TargetPowerSumTo")
                {
                    newPower = process.OperationAritmetic("Temp+=" + value, 0);
                }
                if(action == "TargetAssingTo")
                {
                    newPower = process.OperationAritmetic("Temp=" + value, 0);
                }
                if (newPower != " ") card.Power = int.Parse(newPower);
                process.VarNumber.Remove("Temp");
                return card;
            }
        }
        public void DeterminateList(string lista, Context context, string instruction, string var = " ")
        {
            string list = process.VerificateContextList(lista);
            if (list != " ")
            {
                if (list == "Hand" || (list == "Source" && Source[0].Contains("hand")))
                {
                    if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "hand"))
                    {
                        context.HandOfPlayer_1 = EjecutarEfecto(context.HandOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Hand = CopyList(context.HandOfPlayer_1, context.GameManager.deck1.Hand);
                        for (int i = 0; i < context.GameManager.deck1.Hand.Length; i++)
                        {
                            if (context.GameManager.deck1.Hand[i] != null)
                            {
                                context.GameManager.deck1.HandPosition(i);
                            }
                        }
                    }
                    else if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "otherhand"))
                    {
                        context.HandOfPlayer_2 = EjecutarEfecto(context.HandOfPlayer_2, instruction, var);
                        context.GameManager.deck1.Hand = CopyList(context.HandOfPlayer_2, context.GameManager.deck2.Hand);
                        for (int i = 0; i < context.GameManager.deck2.Hand.Length; i++)
                        {
                            if (context.GameManager.deck2.Hand[i] != null)
                            {
                                context.GameManager.deck2.HandPosition(i);
                            }
                        }
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "hand"))
                    {
                        context.HandOfPlayer_2 = EjecutarEfecto(context.HandOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Hand = CopyList(context.HandOfPlayer_2, context.GameManager.deck2.Hand);
                        for (int i = 0; i < context.GameManager.deck2.Hand.Length; i++)
                        {
                            if (context.GameManager.deck2.Hand[i] != null)
                            {
                                context.GameManager.deck2.HandPosition(i);
                            }

                        }
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "otherhand"))
                    {
                        context.HandOfPlayer_1 = EjecutarEfecto(context.HandOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Hand = CopyList(context.HandOfPlayer_1, context.GameManager.deck1.Hand);
                        for (int i = 0; i < context.GameManager.deck1.Hand.Length; i++)
                        {
                            if (context.GameManager.deck1.Hand[i] != null)
                            {
                                context.GameManager.deck1.HandPosition(i);
                            }

                        }
                    }
                }
                if (list == "Deck" || (list == "Source" && Source[0].Contains("deck")))
                {
                    if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "deck"))
                    {
                        context.DeckOfPlayer_1 = EjecutarEfecto(context.DeckOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Mazo = CopyList(context.DeckOfPlayer_1, context.GameManager.deck1.Mazo);
                    }
                    else if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "otherdeck"))
                    {
                        context.DeckOfPlayer_2 = EjecutarEfecto(context.DeckOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Mazo = CopyList(context.DeckOfPlayer_2, context.GameManager.deck2.Mazo);
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "deck"))
                    {
                        context.DeckOfPlayer_2 = EjecutarEfecto(context.DeckOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Mazo = CopyList(context.DeckOfPlayer_2, context.GameManager.deck1.Mazo);
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "otherdeck"))
                    {
                        context.DeckOfPlayer_1 = EjecutarEfecto(context.DeckOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Mazo = CopyList(context.DeckOfPlayer_1, context.GameManager.deck1.Mazo);
                    }
                }
                if (list == "Graveyard" || (list == "Source" && Source[0].Contains("graveyard")))
                {
                    if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "graveyard"))
                    {
                        context.GraveyardOfPlayer_1 = EjecutarEfecto(context.GraveyardOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Graveyard = (CopyList(context.GraveyardOfPlayer_1, context.GameManager.deck1.Graveyard.ToArray())).ToList();
                    }
                    else if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "othergraveyard"))
                    {
                        context.GraveyardOfPlayer_2 = EjecutarEfecto(context.GraveyardOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Graveyard = (CopyList(context.GraveyardOfPlayer_2, context.GameManager.deck1.Graveyard.ToArray())).ToList();
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "graveyard"))
                    {
                        context.GraveyardOfPlayer_2 = EjecutarEfecto(context.GraveyardOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Graveyard = (CopyList(context.GraveyardOfPlayer_2, context.GameManager.deck2.Graveyard.ToArray())).ToList();
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "othergraveyard"))
                    {
                        context.GraveyardOfPlayer_1 = EjecutarEfecto(context.GraveyardOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Graveyard = (CopyList(context.GraveyardOfPlayer_1, context.GameManager.deck1.Graveyard.ToArray())).ToList();
                    }
                }
                if (list == "Field" || (list == "Source" && Source[0].Contains("field")))
                {
                    if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "field"))
                    {
                        context.FieldOfPlayer_1 = EjecutarEfecto(context.FieldOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Field = CopyList(context.FieldOfPlayer_1, context.GameManager.deck1.Field);
                    }
                    else if (card.TriggerPlayer == 1 || (list == "Source" && Source[0] == "otherfield"))
                    {
                        context.FieldOfPlayer_2 = EjecutarEfecto(context.FieldOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Field = CopyList(context.FieldOfPlayer_2, context.GameManager.deck2.Field);
                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "field"))
                    {
                        context.FieldOfPlayer_2 = EjecutarEfecto(context.FieldOfPlayer_2, instruction, var);
                        context.GameManager.deck2.Field = CopyList(context.FieldOfPlayer_2, context.GameManager.deck2.Field);

                    }
                    else if (card.TriggerPlayer == 2 || (list == "Source" && Source[0] == "otherfield"))
                    {
                        context.FieldOfPlayer_1 = EjecutarEfecto(context.FieldOfPlayer_1, instruction, var);
                        context.GameManager.deck1.Field = CopyList(context.FieldOfPlayer_1, context.GameManager.deck1.Field);

                    }
                }
                if (list == "Board" || (list == "Source" && Source[0] == "board"))
                {
                    context.Board = EjecutarEfecto(context.Board, instruction, var);
                    context.GameManager.Board = CopyList(context.Board, context.GameManager.Board);
                }
            }
        }
        public GameObject[] CopyList(List<GeneralCard> cards, GameObject[] Array)
        {

            for (int i = 0; i < Array.Length; i++)
            {
                if (Array[i] != null)
                {
                    if (!cards.Contains(Array[i].GetComponent<GeneralCard>()))
                    {
                        if (Array[i].GetComponent<GeneralCard>().players == 1) GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().deck1.Graveyard.Add(Array[i]);
                        if (Array[i].GetComponent<GeneralCard>().players == 2) GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().deck2.Graveyard.Add(Array[i]);
                        Array[i] = null;
                    }
                }
            }

            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = null;
            }

            int s = 0;
            foreach (GeneralCard card in cards)
            {
                Array[s] = card.gameObject;
                s++;
            }
            return Array;
        }
    }
}