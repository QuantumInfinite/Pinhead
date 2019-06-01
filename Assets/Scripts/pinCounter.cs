using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class pinCounter : MonoBehaviour {
    Text text;
    int pinCount;
	// Use this for initialization
	void Start () {
        text = GetComponent<Text>();
	}
	// Update is called once per frame
	void FixedUpdate () {
        text.text = string.Format("Pins: {0}", GameObject.FindGameObjectWithTag("Player").GetComponent<FormScript>().pinCount);
    }
}
