using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowCurser : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Locked; //This and next line center curser on screen 
        Cursor.lockState = CursorLockMode.None;   //^
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
