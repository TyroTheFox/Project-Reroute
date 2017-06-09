using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointPool : MonoBehaviour {

    [SerializeField] int startingPoints = 0;
    [SerializeField] Text textDisplay;
    public int currentTotal = 0;

	// Use this for initialization
	void Start () {
        textDisplay.text = currentTotal.ToString();
	}
    
    public void SetTotal(int nt)
    {
        currentTotal = nt;
        textDisplay.text = currentTotal.ToString();
    }

    public void IncreaseTotal(int i)
    {
        currentTotal += i;
        textDisplay.text = currentTotal.ToString();
    }

    public void DecreaseTotal(int i)
    {
        currentTotal -= i;
        textDisplay.text = currentTotal.ToString();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
