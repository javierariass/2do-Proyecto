using CardCompiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Casilla_Invocacion : MonoBehaviour
{
    public int player = 1;
    public int casilla;
    public AttackType attackType;
    public Deck Deck;
    GameManager gameManager;

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
    }
    public void Invocar()
    {
        if (Deck.Card_Invoke != null && !Deck.Card_Invoke.GetComponent<GeneralCard>().Invoke && !gameManager.Invoke && gameManager.Turn == Deck.Player && (Deck.Card_Invoke.GetComponent<GeneralCard>().Type == CardType.Oro || Deck.Card_Invoke.GetComponent<GeneralCard>().Type == CardType.Plata))
        {
            foreach(AttackType att in Deck.Card_Invoke.GetComponent<GeneralCard>().Attack)
            {
                if(att == attackType)
                {
                    Deck.Card_Invoke.transform.position = transform.position;
                    Deck.Card_Invoke.GetComponent<GeneralCard>().Invoke = true;
                    for(int i = 0; i < Deck.Hand.Length; i++)
                    {
                        if(Deck.Card_Invoke == Deck.Hand[i])
                        {
                            Deck.Hand[i] = null;
                        }
                    }
                    Deck.Field[casilla] = Deck.Card_Invoke;
                    Deck.Card_Invoke = null;
                    gameManager.Invoke = true;
                }
            }
        }
        


    }

    private void OnMouseDown()
    {
        Invocar();
    }
}
