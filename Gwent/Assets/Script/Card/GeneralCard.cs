using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardCompiler;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class GeneralCard : MonoBehaviour
{
    public string Name, Faction;
    public float Power;
    public CardType Type;
    public List<AttackType> Attack;
    public RawImage Imag_Des,Text_Des_Img;
    public TextMeshProUGUI Text_Des;
    public Deck Deck;
    public bool invoke,effects = false;
    public effect effect;
    public int players;

    private void Start()
    {
        Imag_Des = GameObject.FindGameObjectWithTag("Img_Des").GetComponent<RawImage>();
        Text_Des_Img = GameObject.FindGameObjectWithTag("Img_DesText").GetComponent<RawImage>();
        Text_Des = GameObject.FindGameObjectWithTag("Text_Des").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if(invoke && !effects && effect != null)
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().context.ejecutar(effect,players);
            effects = true;
        }
    }
    public void Create(Card carta,Deck player,int Player)
    {
        Deck = player;
        players = Player;
        Name = carta.Name;
        Faction = carta.Faction;
        Power = carta.Power;
        Type = carta.Type;
        Attack = carta.AttackType;
        effect = carta.effect;

        gameObject.name = Name;
        gameObject.transform.localScale = new Vector3(0.0784518644f, 0.0814131051f, 1);

        gameObject.AddComponent<SpriteRenderer>();
        gameObject.AddComponent<BoxCollider2D>();

        gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/" + Name);
        gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(9.63649f, 12.67119f);
    }

    private void OnMouseEnter()
    {
        Imag_Des.transform.localScale = Vector3.one;
        Text_Des_Img.transform.localScale = Vector3.one;
        Imag_Des.texture = GetComponent<SpriteRenderer>().sprite.texture;
        string ataques = "";
        foreach(AttackType s in Attack)
        {
            ataques += s.ToString() + " ";
        }
        Text_Des.text = "Name: " + Name + "\n" + "Type: " + Type + "\n" + "Faction: " + Faction + "\n" + "Power: " + Power + "\n" + "Attack: " + ataques;
    }

    private void OnMouseExit()
    {
        Imag_Des.transform.localScale = Vector3.zero;
        Text_Des_Img.transform.localScale = Vector3.zero;
    }

    private void OnMouseDown()
    {
        Deck.Select_Invoke(gameObject);
    }
}
