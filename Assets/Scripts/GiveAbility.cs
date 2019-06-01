using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveAbility : MonoBehaviour {
    public FormScript.Form selectedForm;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            GameObject.FindGameObjectWithTag("UI").GetComponent<PauseMenu>().EnableForm(selectedForm);
            Destroy(gameObject);
        }
    }
}
