using CardCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Context : MonoBehaviour
{
    public List<GeneralCard> Board;
    public List<GeneralCard> HandOfPlayer_1;
    public List<GeneralCard> FieldOfPlayer_1;
    public List<GeneralCard> GraveyardOfPlayer_1;
    public List<GeneralCard> DeckOfPlayer_1;
    public List<GeneralCard> HandOfPlayer_2;
    public List<GeneralCard> FieldOfPlayer_2;
    public List<GeneralCard> GraveyardOfPlayer_2;
    public List<GeneralCard> DeckOfPlayer_2;

    public GameManager GameManager;

    public void ejecutar(effect Effect,int player)
    {
        GameManager.DeterminateContext();
        Dictionary<string, GeneralCard> ParamEffect = new();
        string[] partes;
        string[] mazo;
        foreach (string instrucion in Effect.Instruntions)
        {
            partes = instrucion.Split('|');
            switch (partes[0])
            {
                case "Pop":
                    if (partes[2] == "Hand")
                    {
                        if(player == 1)
                        {
                            ParamEffect[partes[1]] = HandOfPlayer_1[0];
                            ParamEffect[partes[1]].gameObject.transform.position = new Vector2(1000, 1000);
                            HandOfPlayer_1.RemoveAt(0);
                            for (int i = 0; i < 10; i++)
                            {
                                GameManager.deck1.Hand[i] = null;
                            }
                            for (int i = 0; i < HandOfPlayer_1.Count; i++)
                            {
                                GameManager.deck1.Hand[i] = HandOfPlayer_1[i].gameObject;
                                GameManager.deck1.HandPosition(i);
                            }
                        }
                        if (player == 2)
                        {
                            ParamEffect[partes[1]] = HandOfPlayer_2[0];
                            HandOfPlayer_2.RemoveAt(0);
                            for (int i = 0; i < 10; i++)
                            {
                                GameManager.deck2.Hand[i] = null;
                            }
                            for (int i = 0; i < HandOfPlayer_2.Count; i++)
                            {
                                GameManager.deck2.Hand[i] = HandOfPlayer_2[i].gameObject;
                                GameManager.deck2.HandPosition(i);
                            }
                        }
                    }
                    if (partes[2] == "Deck")
                    {
                        if(player == 1)
                        {
                            ParamEffect[partes[1]] = DeckOfPlayer_1[0];
                            DeckOfPlayer_1.RemoveAt(0);
                            for (int i = 0; i < GameManager.deck1.Mazo.Length; i++)
                            {
                                GameManager.deck1.Mazo[i] = null;
                            }
                            for (int i = 0; i < DeckOfPlayer_1.Count; i++)
                            {
                                GameManager.deck1.Mazo[i] = DeckOfPlayer_1[i].gameObject;
                            }
                        }
                        if (player == 2)
                        {
                            ParamEffect[partes[1]] = DeckOfPlayer_2[0];
                            DeckOfPlayer_2.RemoveAt(0);
                            for (int i = 0; i < GameManager.deck2.Mazo.Length; i++)
                            {
                                GameManager.deck2.Mazo[i] = null;
                            }
                            for (int i = 0; i < DeckOfPlayer_2.Count; i++)
                            {
                                GameManager.deck2.Mazo[i] = DeckOfPlayer_2[i].gameObject;
                            }
                        }
                    }
                    if (partes[2] == "Graveyard")
                    {
                        if (player == 1)
                        {
                            ParamEffect[partes[1]] = GraveyardOfPlayer_1[0];
                            GraveyardOfPlayer_1.RemoveAt(0);
                            for (int i = 0; i < GameManager.deck1.Graveyard.Length; i++)
                            {
                                GameManager.deck1.Graveyard[i] = null;
                            }
                            for (int i = 0; i < GraveyardOfPlayer_1.Count; i++)
                            {
                                GameManager.deck1.Graveyard[i] = GraveyardOfPlayer_1[i].gameObject;
                            }
                        }
                        if (player == 2)
                        {
                            ParamEffect[partes[1]] = GraveyardOfPlayer_2[0];
                            GraveyardOfPlayer_2.RemoveAt(0);
                            for (int i = 0; i < GameManager.deck2.Graveyard.Length; i++)
                            {
                                GameManager.deck2.Graveyard[i] = null;
                            }
                            for (int i = 0; i < GraveyardOfPlayer_2.Count; i++)
                            {
                                GameManager.deck2.Graveyard[i] = GraveyardOfPlayer_2[i].gameObject;
                            }
                        }
                    }
                    break;

                case "Shufle":
                    if (partes[1] == "Hand")
                    {
                        if(player == 1)
                        {
                            GameManager.deck1.Hand = GameManager.deck1.Barajear(GameManager.deck1.Hand);
                            for (int i = 0; i < 10; i++)
                            {
                                if (GameManager.deck1.Hand[i] != null)
                                {
                                    GameManager.deck1.HandPosition(i);
                                }
                            }
                        }
                        if (player == 2)
                        {
                            GameManager.deck2.Hand = GameManager.deck1.Barajear(GameManager.deck2.Hand);
                            for (int i = 0; i < 10; i++)
                            {
                                if (GameManager.deck2.Hand[i] != null)
                                {
                                    GameManager.deck2.HandPosition(i);
                                }
                            }
                        }
                    }
                    if (partes[1] == "Deck")
                    {
                        if(player == 1)
                        {
                            GameManager.deck1.Mazo = GameManager.deck1.Barajear(GameManager.deck1.Mazo);
                        }
                        if (player == 2)
                        {
                            GameManager.deck2.Mazo = GameManager.deck2.Barajear(GameManager.deck2.Mazo);
                        }
                    }
                    if (partes[1] == "Graveyard")
                    {
                        if (player == 1)
                        {
                            GameManager.deck1.Graveyard = GameManager.deck1.Barajear(GameManager.deck1.Graveyard);
                        }
                        if (player == 2)
                        {
                            GameManager.deck2.Graveyard = GameManager.deck2.Barajear(GameManager.deck2.Graveyard);
                        }
                    }
                    break;

                case "Push":
                    if (partes[1] == "Hand")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                if (GameManager.deck1.Hand[0] == null)
                                {
                                    GameManager.deck1.Hand[0] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck1.HandPosition(0);
                                }
                                else if (GameManager.deck1.Hand[0] != null)
                                {
                                    Destroy(GameManager.deck1.Hand[0]);
                                    GameManager.deck1.Hand[0] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck1.HandPosition(0);
                                    break;
                                }
                            }
                            if (player == 2)
                            {
                                if (GameManager.deck2.Hand[0] == null)
                                {
                                    GameManager.deck2.Hand[0] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck2.HandPosition(0);
                                }
                                else if (GameManager.deck2.Hand[0] != null)
                                {
                                    Destroy(GameManager.deck2.Hand[0]);
                                    GameManager.deck2.Hand[0] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck2.HandPosition(0);
                                    break;
                                }
                            }

                        }
                    }
                    if (partes[1] == "Deck")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                if (GameManager.deck1.Mazo[0] == null)
                                {
                                    GameManager.deck1.Mazo[0] = ParamEffect[partes[2]].gameObject;
                                }
                                else if (GameManager.deck1.Mazo[0] != null)
                                {
                                    Destroy(GameManager.deck1.Mazo[0]);
                                    GameManager.deck1.Mazo[0] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }
                            if (player == 2)
                            {
                                if (GameManager.deck2.Mazo[0] == null)
                                {
                                    GameManager.deck2.Mazo[0] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck2.HandPosition(0);
                                }
                                else if (GameManager.deck2.Mazo[0] != null)
                                {
                                    Destroy(GameManager.deck2.Hand[0]);
                                    GameManager.deck2.Mazo[0] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }


                        }
                    }
                    if (partes[1] == "Graveyard")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                if (GameManager.deck1.Graveyard[0] == null)
                                {
                                    GameManager.deck1.Graveyard[0] = ParamEffect[partes[2]].gameObject;
                                }
                                else if (GameManager.deck1.Graveyard[0] != null)
                                {
                                    Destroy(GameManager.deck1.Graveyard[0]);
                                    GameManager.deck1.Graveyard[0] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }
                            if (player == 2)
                            {
                                if (GameManager.deck2.Graveyard[0] == null)
                                {
                                    GameManager.deck2.Graveyard[0] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck2.HandPosition(0);
                                }
                                else if (GameManager.deck2.Graveyard[0] != null)
                                {
                                    Destroy(GameManager.deck2.Graveyard[0]);
                                    GameManager.deck2.Graveyard[0] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }


                        }
                    }
                    break;

                case "SendBottom":
                    if (partes[1] == "Hand")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                if (GameManager.deck1.Hand[GameManager.deck1.Hand.Length-1] == null)
                                {
                                    GameManager.deck1.Hand[GameManager.deck1.Hand.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck1.HandPosition(GameManager.deck1.Hand.Length - 1);
                                }
                                else if (GameManager.deck1.Hand[GameManager.deck1.Hand.Length - 1] != null)
                                {
                                    Destroy(GameManager.deck1.Hand[GameManager.deck1.Hand.Length - 1]);
                                    GameManager.deck1.Hand[GameManager.deck1.Hand.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck1.HandPosition(GameManager.deck1.Hand.Length - 1);
                                    break;
                                }
                            }
                            if (player == 2)
                            {
                                if (GameManager.deck2.Hand[GameManager.deck2.Hand.Length - 1] == null)
                                {
                                    GameManager.deck2.Hand[GameManager.deck2.Hand.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck2.HandPosition(GameManager.deck2.Hand.Length - 1);
                                }
                                else if (GameManager.deck2.Hand[GameManager.deck2.Hand.Length - 1] != null)
                                {
                                    Destroy(GameManager.deck2.Hand[GameManager.deck2.Hand.Length - 1]);
                                    GameManager.deck2.Hand[GameManager.deck2.Hand.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    GameManager.deck2.HandPosition(GameManager.deck2.Hand.Length - 1);
                                    break;
                                }
                            }

                        }
                    }
                    if (partes[1] == "Deck")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                if (GameManager.deck1.Mazo[GameManager.deck1.Mazo.Length - 1] == null)
                                {
                                    GameManager.deck1.Mazo[GameManager.deck1.Mazo.Length - 1] = ParamEffect[partes[2]].gameObject;
                                }
                                else if (GameManager.deck1.Mazo[GameManager.deck1.Mazo.Length - 1] != null)
                                {
                                    Destroy(GameManager.deck1.Mazo[GameManager.deck1.Mazo.Length - 1]);
                                    GameManager.deck1.Mazo[GameManager.deck1.Mazo.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }
                            if (player == 2)
                            {
                                if (GameManager.deck2.Mazo[GameManager.deck2.Mazo.Length - 1] == null)
                                {
                                    GameManager.deck2.Mazo[GameManager.deck2.Mazo.Length - 1] = ParamEffect[partes[2]].gameObject;
                                }
                                else if (GameManager.deck2.Mazo[GameManager.deck2.Mazo.Length - 1] != null)
                                {
                                    Destroy(GameManager.deck2.Hand[GameManager.deck2.Mazo.Length - 1]);
                                    GameManager.deck2.Mazo[GameManager.deck2.Mazo.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }


                        }
                    }
                    if (partes[1] == "Graveyard")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                if (GameManager.deck1.Graveyard[GameManager.deck1.Graveyard.Length - 1] == null)
                                {
                                    GameManager.deck1.Graveyard[GameManager.deck1.Graveyard.Length - 1] = ParamEffect[partes[2]].gameObject;
                                }
                                else if (GameManager.deck1.Graveyard[GameManager.deck1.Graveyard.Length - 1] != null)
                                {
                                    Destroy(GameManager.deck1.Graveyard[GameManager.deck1.Graveyard.Length - 1]);
                                    GameManager.deck1.Graveyard[GameManager.deck1.Graveyard.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }
                            if (player == 2)
                            {
                                if (GameManager.deck2.Graveyard[GameManager.deck2.Graveyard.Length - 1] == null)
                                {
                                    GameManager.deck2.Graveyard[GameManager.deck2.Graveyard.Length - 1] = ParamEffect[partes[2]].gameObject;
                                }
                                else if (GameManager.deck2.Graveyard[GameManager.deck2.Graveyard.Length - 1] != null)
                                {
                                    Destroy(GameManager.deck2.Hand[GameManager.deck2.Graveyard.Length - 1]);
                                    GameManager.deck2.Graveyard[GameManager.deck2.Graveyard.Length - 1] = ParamEffect[partes[2]].gameObject;
                                    break;
                                }
                            }


                        }
                    }
                    break;

                case "Add":
                    if (partes[1] == "Hand")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                for(int i = 0; i < 10; i++)
                                {
                                    if (GameManager.deck1.Hand[i] == null)
                                    {
                                        GameManager.deck1.Hand[i] = ParamEffect[partes[2]].gameObject;
                                        GameManager.deck1.HandPosition(i);
                                        break;
                                    }
                                }
                            }
                            if (player == 2)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    if (GameManager.deck2.Hand[i] == null)
                                    {
                                        GameManager.deck2.Hand[i] = ParamEffect[partes[2]].gameObject;
                                        GameManager.deck2.HandPosition(i);
                                        break;
                                    }
                                }
                            }

                        }
                    }
                    if (partes[1] == "Deck")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                for (int i = 0; i < GameManager.deck1.Mazo.Length; i++)
                                {
                                    if (GameManager.deck1.Mazo[i] == null)
                                    {
                                        GameManager.deck1.Mazo[i] = ParamEffect[partes[2]].gameObject;
                                        break;
                                    }
                                }
                            }
                            if (player == 2)
                            {
                                for (int i = 0; i < GameManager.deck1.Mazo.Length; i++)
                                {
                                    if (GameManager.deck2.Mazo[i] == null)
                                    {
                                        GameManager.deck2.Mazo[i] = ParamEffect[partes[2]].gameObject;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (partes[1] == "Graveyard")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                for (int i = 0; i < GameManager.deck1.Graveyard.Length; i++)
                                {
                                    if (GameManager.deck1.Graveyard[i] == null)
                                    {
                                        GameManager.deck1.Graveyard[i] = ParamEffect[partes[2]].gameObject;
                                        break;
                                    }
                                }
                            }
                            if (player == 2)
                            {
                                for (int i = 0; i < GameManager.deck1.Graveyard.Length; i++)
                                {
                                    if (GameManager.deck2.Graveyard[i] == null)
                                    {
                                        GameManager.deck2.Graveyard[i] = ParamEffect[partes[2]].gameObject;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;

                case "Remove":
                    if (partes[1] == "Hand")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    if (GameManager.deck1.Hand[i] == ParamEffect[partes[2]].gameObject)
                                    {
                                        Destroy(GameManager.deck1.Hand[i]);
                                        GameManager.deck1.Hand[i] = null;
                                        break;
                                    }
                                }
                            }
                            if (player == 2)
                            {
                                for (int i = 0; i < 10; i++)
                                {
                                    if (GameManager.deck2.Hand[i] == ParamEffect[partes[2]].gameObject)
                                    {
                                        Destroy(GameManager.deck2.Hand[i]);
                                        GameManager.deck2.Hand[i] = null;
                                        break;
                                    }
                                }
                            }

                        }
                    }
                    if (partes[1] == "Deck")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                for (int i = 0; i < GameManager.deck1.Mazo.Length; i++)
                                {
                                    if (GameManager.deck1.Mazo[i] == ParamEffect[partes[2]].gameObject)
                                    {
                                        Destroy(GameManager.deck1.Mazo[i]);
                                        GameManager.deck1.Mazo[i] = null;
                                        break;
                                    }
                                }
                            }
                            if (player == 2)
                            {
                                for (int i = 0; i < GameManager.deck1.Mazo.Length; i++)
                                {
                                    if (GameManager.deck2.Mazo[i] == ParamEffect[partes[2]].gameObject)
                                    {
                                        Destroy(GameManager.deck2.Mazo[i]);
                                        GameManager.deck2.Mazo[i] = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (partes[1] == "Graveyard")
                    {
                        if (Effect.Local_param.Contains(partes[2]))
                        {
                            if (player == 1)
                            {
                                for (int i = 0; i < GameManager.deck1.Graveyard.Length; i++)
                                {
                                    if (GameManager.deck1.Graveyard[i] == ParamEffect[partes[2]].gameObject)
                                    {
                                        Destroy(GameManager.deck1.Graveyard[i]);
                                        GameManager.deck1.Graveyard[i] = null;
                                        break;
                                    }
                                }
                            }
                            if (player == 2)
                            {
                                for (int i = 0; i < GameManager.deck1.Graveyard.Length; i++)
                                {
                                    if (GameManager.deck2.Graveyard[i] == ParamEffect[partes[2]].gameObject)
                                    {
                                        Destroy(GameManager.deck2.Graveyard[i]);
                                        GameManager.deck2.Graveyard[i] = null;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}


