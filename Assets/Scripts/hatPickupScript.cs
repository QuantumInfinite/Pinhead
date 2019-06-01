using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hatPickupScript : MonoBehaviour {

    public enum Hat {
        pinhead,
        spindle,
        claydough,
        rebutia
    }
    public Hat hatToGivePlayers;
    GameObject particles;
    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            switch (hatToGivePlayers) {
                case Hat.pinhead:
                    other.GetComponent<FormScript>().hasHatPinhead = true;
                    break;
                case Hat.spindle:
                    other.GetComponent<FormScript>().hasHatSpindle = true;
                    break;
                case Hat.claydough:
                    other.GetComponent<FormScript>().hasHatClaydough = true;
                    break;
                case Hat.rebutia:
                    other.GetComponent<FormScript>().hasHatRebutia = true;
                    break;
            }
            other.GetComponent<FormScript>().SetHat(other.GetComponent<FormScript>().currentForm);
            particles.SetActive(false);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<AudioSource>().Play();
        }
    }
    // Use this for initialization
    void Start () {
        particles = transform.GetChild(0).gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
