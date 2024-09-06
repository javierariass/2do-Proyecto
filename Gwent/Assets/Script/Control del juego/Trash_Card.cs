using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Trash_Card : MonoBehaviour
{
    public Start_Game game;
    public int num = 0;

    //Funcion para botar cartas
    public void Activar()
    {
        if(GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().Turn == 1)
        {
            game.Cambiar1(num, GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().deck1.GetComponent<Deck>());

        }
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().Turn == 2)
        {
            game.Cambiar1(num, GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().deck2.GetComponent<Deck>());

        }
    }
}
