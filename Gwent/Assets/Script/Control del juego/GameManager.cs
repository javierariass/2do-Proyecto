using CardCompiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    string datos;
    public List<GameObject> Cards = new();
    public GameObject LeaderSelect;
    public GameObject Deck1, Deck2;
    public Transform Pos1, Pos2;
    public Deck deck1, deck2;
    public Camera P1, P2;
    public bool invoke = false;
    public int Turn = 1;
    public TextMeshProUGUI P1Round,P2Round,P1Power,P2Power;
    public float Round1,Round2, RoundPower1,RoundPower2;
    public GameObject[] Climas,Climas_Pos = new GameObject[3];
    public bool Jug1_End, Jug2_End,Game_End = false;
    public bool inicio1, inicio2 = true;
    public Context context;
    public GameObject[] Board = new GameObject[33];

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0)) GetComponent<AudioSource>().Play();
        PowerInCamp();
        EndRound();
        EndGame();
        DeterminateContext();      
    }

    //Funcion de Inicializar juego 
    private void StartGame()
    {
        string[] Players = File.ReadAllText(Application.dataPath + "/Resources/Setting/PlayerSetting.txt").Split('|');

        LoadDeck(Players[0], Deck1, Pos1,deck1,1);
        deck1.Mazo = new GameObject[Cards.Count];
        deck1.Mazo = Cards.ToArray();
        deck1.Mazo = deck1.Barajear(deck1.Mazo);
        deck1.Leader = LeaderSelect;
        deck1.Leader.transform.position = deck1.Field_leader.transform.position;
        LeaderSelect = null;
        Cards.Clear();

        LoadDeck(Players[1], Deck2, Pos2,deck2,2);
        deck2.Mazo = new GameObject[Cards.Count];
        deck2.Mazo = Cards.ToArray();
        Cards.Clear();
        deck2.Mazo = deck2.Barajear(deck2.Mazo);
        deck2.Leader = LeaderSelect;
        deck2.Leader = LeaderSelect;
        deck2.Leader.transform.position = deck2.Field_leader.transform.position;
        deck2.Leader.transform.rotation = deck2.Field_leader.transform.rotation;
        LeaderSelect = null;
    }

    //Funcion para cargar decks
    private void LoadDeck(string decks,GameObject deckPos,Transform positions, Deck player,int TriggerPlayer)
    {
        datos = File.ReadAllText(Application.dataPath + "/Resources/Decks/" + decks + ".txt");
        LexicalProcess process = new();
        string[] cartas = datos.Split('\n');
        foreach (string cart in cartas)
        {
            string[] deck = cart.Split('|');
            GameObject gameObject = new();
            List<AttackType> attack = new();
            string[] ataques = deck[4].Split('-');
            foreach (string a in ataques)
            {
                if (process.CompareAttackType(a) != AttackType.None)
                {
                    attack.Add(process.CompareAttackType(a));
                }
            }
            if (deck[1] != "Oro" && deck[1] != "Plata")
            {
                deck[3] = "0";
            }
            Card card = new(deck[0], process.CompareCardType(deck[1]), deck[2], int.Parse(deck[3]), attack,TriggerPlayer);
            if(deck.Length == 6 && File.Exists(Application.dataPath + "/Resources/Effects/" + deck[5].Split('*')[0] + ".txt"))
            {
                string effec = File.ReadAllText(Application.dataPath + "/Resources/Effects/" + deck[5].Split('*')[0] + ".txt");
                card.effect = new effect(effec, deck[5].Split('*')[2],card,deck[5].Split('*')[1]);
            }
            gameObject.AddComponent<GeneralCard>();
            gameObject.GetComponent<GeneralCard>().Create(card,player,TriggerPlayer);
            gameObject.transform.parent = deckPos.transform;
            gameObject.transform.position = positions.position;
            if(card.Type == CardType.Lider)
            {
                LeaderSelect = gameObject;
            }
            else
            {
                Cards.Add(gameObject);
            }
        }
    }

    //Funcion para cambiar turno
    public void ChangeTurn()
    {
        bool cam_Change = false;
        if (!invoke && Turn == 1)
        {
            Jug1_End = true;
        }
        if (!invoke && Turn == 2)
        {
            Jug2_End = true;
        }
        TextMeshProUGUI text;
        if(P1.isActiveAndEnabled && !Jug2_End && !inicio1)
        {
            P1.gameObject.SetActive(false);
            P2.gameObject.SetActive(true);
            cam_Change = true;
            Turn = 2;
            for(int i=0;i<Climas_Pos.Length;i++)
            {
                Climas_Pos[i].GetComponent<Casilla_Invocacion>().player = 2;
                Climas_Pos[i].GetComponent<Casilla_Invocacion>().Deck = deck2;
            }          
        }
        else if(P2.isActiveAndEnabled && !Jug1_End && !inicio2)
        {

            P2.gameObject.SetActive(false);
            P1.gameObject.SetActive(true);
            cam_Change = true;
            Turn = 1;
            for (int i = 0; i < Climas_Pos.Length; i++)
            {
                Climas_Pos[i].GetComponent<Casilla_Invocacion>().player = 1;
                Climas_Pos[i].GetComponent<Casilla_Invocacion>().Deck = deck1;
            }
        }

        if(cam_Change)
        {
            text = P1Power;
            P1Power = P2Power;
            P2Power = text;

        }
        if ((!Jug1_End && !inicio1)|| (!Jug2_End && !inicio2))
        {
            invoke = false;
        }
    }

    //Funcion para verificar fin de rondas
    public void EndRound()
    {
        if (Jug1_End && Jug2_End && !Game_End)
        {
            if (RoundPower1 > RoundPower2)
            {
                Round1++;
            }
            else if (RoundPower1 < RoundPower2)
            {
                Round2++;
            }
            else if(RoundPower2 == RoundPower1)
            {
                Round2++;
                Round1++;
            }
            P1Round.text = "P1: " + Round1;
            P2Round.text = "P2: " + Round2;
            Jug1_End = false;
            Jug2_End = false;

            for(int i = 0; i < Climas_Pos.Length; i++)
            {
                if (Climas[i] != null)
                {
                    if (Climas[i].GetComponent<GeneralCard>().players == 1) deck1.Graveyard.Add(Climas[i]);
                    if (Climas[i].GetComponent<GeneralCard>().players == 2) deck2.Graveyard.Add(Climas[i]);
                }
                Climas[i] = null;
            }

            for(int i = 0; i < deck1.Field.Length; i++)
            {
                if (deck1.Field[i] != null) deck1.Graveyard.Add(deck1.Field[i]);
                deck1.Field[i] = null;
                if (deck2.Field[i] != null) deck2.Graveyard.Add(deck2.Field[i]);
                deck2.Field[i] = null;
            }

            for (int i = 0; i < deck1.Aum.Length; i++)
            {
                if (deck1.Aum[i] != null) deck1.Graveyard.Add(deck1.Aum[i]);
                deck1.Aum[i] = null;
                if (deck2.Aum[i] != null) deck2.Graveyard.Add(deck2.Aum[i]);
                deck2.Aum[i] = null;
            }
            deck1.Robar(2);
            deck2.Robar(2);
        }
    }

    //Funcion para verificar poder en el campo
    public void PowerInCamp()
    {
        float i = 0;
        float s = 0;
        for (int a = 0; a < 12; a++)
        {
            if (deck1.Field[a] != null)
            {
                i += deck1.Field[a].GetComponent<GeneralCard>().Power;
            }
            if (deck2.Field[a] != null)
            {
                s += deck2.Field[a].GetComponent<GeneralCard>().Power;
            }
        }
        RoundPower1 = i;
        RoundPower2 = s;

        P1Power.text = "Poder: " + RoundPower1;
        P2Power.text = "Poder: " + RoundPower2;
    }

    //Funcion para ver fin del juego
    public void EndGame()
    {
        if(Round1 == 2 && Round2 < 2)
        {
            invoke = true;
            Jug1_End = true;
            Jug2_End = true;
            Game_End = true;
        }

        if (Round1 < 2 && Round2 == 2)
        {
            invoke = true;
            Jug1_End = true;
            Jug2_End = true;
            Game_End = true;
        }

        if (Round1 == 2 && Round2 == 2)
        {
            invoke = true;
            Jug1_End = true;
            Jug2_End = true;
            Game_End = true;
        }
    }

    
    public void DeterminateContext()
    {
        context.Board.Clear();
        context.HandOfPlayer_1.Clear();
        context.FieldOfPlayer_1.Clear();
        context.GraveyardOfPlayer_1.Clear();
        context.DeckOfPlayer_1.Clear();
        context.HandOfPlayer_2.Clear();
        context.FieldOfPlayer_2.Clear();
        context.GraveyardOfPlayer_2.Clear();
        context.DeckOfPlayer_2.Clear();

        //Cartas en la mano de cada jugador;
        for (int i = 0; i < 10; i++)
        {
            if (deck1.Hand[i] != null)
            {
                context.HandOfPlayer_1.Add(deck1.Hand[i].GetComponent<GeneralCard>());
            }
            if (deck2.Hand[i] != null)
            {
                context.HandOfPlayer_2.Add(deck2.Hand[i].GetComponent<GeneralCard>());
            }
        }

        //Cartas del campo
        for (int i = 0; i < 12; i++)
        {
            if (deck1.Field[i] != null)
            {
                context.Board.Add(deck1.Field[i].GetComponent<GeneralCard>());
            }
            if (deck2.Field[i] != null)
            {
                context.Board.Add(deck2.Field[i].GetComponent<GeneralCard>());
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (deck1.Aum[i] != null)
            {
                context.Board.Add(deck1.Aum[i].GetComponent<GeneralCard>());
            }
            if (deck2.Aum[i] != null)
            {
                context.Board.Add(deck2.Aum[i].GetComponent<GeneralCard>());
            }
            if (Climas[i] != null)
            {
                context.Board.Add(Climas[i].GetComponent<GeneralCard>());
            }
        }
        //Cartas del deck1
        for(int i = 0; i < deck1.Mazo.Length;i++)
        {
            if (deck1.Mazo[i] != null)
            {
                context.DeckOfPlayer_1.Add(deck1.Mazo[i].GetComponent<GeneralCard>());
            }
        }

        //Cartas del deck2
        for (int i = 0; i < deck2.Mazo.Length; i++)
        {
            if (deck2.Mazo[i] != null)
            {
                context.DeckOfPlayer_2.Add(deck2.Mazo[i].GetComponent<GeneralCard>());
            }
        }

        //Cartas del field1
        for (int i = 0; i < deck1.Field.Length; i++)
        {
            if (deck1.Field[i] != null)
            {
                context.FieldOfPlayer_1.Add(deck1.Field[i].GetComponent<GeneralCard>());
            }
        }
        //Cartas del field2
        for (int i = 0; i < deck2.Field.Length; i++)
        {
            if (deck2.Field[i] != null)
            {
                context.FieldOfPlayer_2.Add(deck2.Field[i].GetComponent<GeneralCard>());
            }
        }

        //Cartas del Graveyard1
        for (int i = 0; i < deck1.Graveyard.Count; i++)
        {
            if (deck1.Graveyard[i] != null)
            {
                context.GraveyardOfPlayer_1.Add(deck1.Graveyard[i].GetComponent<GeneralCard>());
            }
        }
        //Cartas del Graveyard2
        for (int i = 0; i < deck2.Graveyard.Count; i++)
        {
            if (deck2.Graveyard[i] != null)
            {
                context.GraveyardOfPlayer_2.Add(deck2.Graveyard[i].GetComponent<GeneralCard>());
            }
        }
    }

}


