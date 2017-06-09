using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UIUpdater : MonoBehaviour {

    public enum Stat { Agility, Constitution, Dexterity };
    [SerializeField] public Stat stat;
    [SerializeField] Text statName;
    [SerializeField] Text statDisplay;

    float statValue = 0;

    GameObject[] players;

    // Use this for initialization
    void Start () {        
        if (stat == Stat.Agility)
        {
            statName.text = "Agility";
            statDisplay.text = statValue.ToString();
        }
        if (stat == Stat.Constitution)
        {
            statName.text = "Constitution";
            statDisplay.text = statValue.ToString();
        }
        if (stat == Stat.Dexterity)
        {
            statName.text = "Dexterity";
            statDisplay.text = statValue.ToString();
        }
    }

    public void UpdateStatValue(float sv)
    { statValue = sv; statDisplay.text = statValue.ToString(); }
	
	// Update is called once per frame
	void Update () {}
}
