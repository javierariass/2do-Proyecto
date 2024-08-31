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

}


