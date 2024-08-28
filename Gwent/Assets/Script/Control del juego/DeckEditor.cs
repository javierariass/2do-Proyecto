using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.UI;

public class DeckEditor : MonoBehaviour
{
    public TMP_Dropdown player1, player2;
    public TextMeshProUGUI p1, p2;
    // Start is called before the first frame update
    void Start()
    {
        Options();
    }

    
    public void Options()
    {
        string decks = File.ReadAllText(Application.dataPath + "/Resources/Setting/DeckInGame.txt");
        string[] Decks = decks.Split('-');
        List<TMP_Dropdown.OptionData> Options = new();
        foreach(string Deck in Decks)
        {
            if(File.ReadAllText(Application.dataPath + "/Resources/Decks/" + Deck + ".txt").Split('\n').Length >24)
            {
                Options.Add(new TMP_Dropdown.OptionData(Deck));
            }         
        }
        player1.AddOptions(Options);
        player2.AddOptions(Options);
    }

    public void Save()
    {
        StreamWriter sw = new(Application.dataPath + "/Resources/Setting/PlayerSetting.txt");
        sw.Write(p1.text + "|" + p2.text);
        sw.Close();
    }
}
