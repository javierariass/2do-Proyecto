using CardCompiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
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
    public bool Invoke = false;
    public int Turn = 1;
    public TextMeshProUGUI P1Round,P2Round,P1Power,P2Power;
    public float Round1,Round2, RoundPower1,RoundPower2;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    private void Update()
    {
        PowerInCamp();
    }

    private void StartGame()
    {
        string[] Players = File.ReadAllText(Application.dataPath + "/Resources/Setting/PlayerSetting.txt").Split('|');

        LoadDeck(Players[0], Deck1, Pos1,deck1);
        deck1.Mazo = new GameObject[Cards.Count];
        deck1.Mazo = Cards.ToArray();
        deck1.Barajear();
        deck1.Robar(10);
        deck1.Leader = LeaderSelect;
        deck1.Leader.transform.position = deck1.Field_leader.transform.position;
        LeaderSelect = null;
        Cards.Clear();

        LoadDeck(Players[1], Deck2, Pos2,deck2);
        deck2.Mazo = new GameObject[Cards.Count];
        deck2.Mazo = Cards.ToArray();
        Cards.Clear();
        deck2.Barajear();
        deck2.Robar(10);
        deck2.Leader = LeaderSelect;
        deck2.Leader = LeaderSelect;
        deck2.Leader.transform.position = deck2.Field_leader.transform.position;
        deck2.Leader.transform.rotation = deck2.Field_leader.transform.rotation;
        LeaderSelect = null;
    }

    private void LoadDeck(string decks,GameObject deckPos,Transform positions, Deck player)
    {
        datos = File.ReadAllText(Application.dataPath + "/Resources/Decks/" + decks + ".txt");
        LexicalProcess process = new();
        string[] cartas = datos.Split(';');
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
            Card card = new Card(deck[0], process.CompareCardType(deck[1]), deck[2], int.Parse(deck[3]), attack);
            gameObject.AddComponent<GeneralCard>();
            gameObject.GetComponent<GeneralCard>().Create(card,player);
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

    public void ChangeTurn()
    {
        TextMeshProUGUI text;
        if(P1.isActiveAndEnabled)
        {
            P1.gameObject.SetActive(false);
            P2.gameObject.SetActive(true);           
            Turn = 2;
        }
        else
        {

            P2.gameObject.SetActive(false);
            P1.gameObject.SetActive(true);
            Turn = 1;
        }
        text = P1Power;
        P1Power = P2Power;
        P2Power = text;
        Invoke = false;
    }

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
}
