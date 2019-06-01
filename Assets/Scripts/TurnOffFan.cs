using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TurnOffFan : MonoBehaviour {
    // Chris wrote this........
    Animator anima;
    public GameObject fan;
    public GameObject[] streamers;
	// Use this for initialization
	void Start () {
        anima = fan.GetComponent<Animator>();
	}
	
	// Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.tag == "ruler")
        {
            anima.SetTrigger("TurnOffFan");

            //turning off streamers
            for (int i = 0; i < streamers.Length; i++)
            {
               Cloth c = streamers[i].GetComponent<Cloth>();
                c.externalAcceleration = new Vector3(0, 0, 0);
                c.randomAcceleration = new Vector3(0, 0, 0);
            }
        }
    }
}
