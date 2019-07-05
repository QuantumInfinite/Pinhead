using UnityEngine;

//ATTACH TO MAIN CAMERA, shows your health and coins
public class GUIManager : MonoBehaviour
{
    public GUISkin guiSkin;					//assign the skin for GUI display
    public bool debugStuff = false;
    void OnGUI()
    {

        GUI.skin = guiSkin;
        GUILayout.Space(5f);
        if (debugStuff)
        {
            GUILayout.Label("Pins: " + GameObject.FindGameObjectWithTag("Player").GetComponent<FormScript>().pinCount);
            GUILayout.Label("Current Form: " + GameObject.FindGameObjectWithTag("Player").GetComponent<FormScript>().currentForm);

            //GUILayout.Label("AbilityActive: " + GameObject.FindGameObjectWithTag("Player").GetComponent<FormScript>().abilityIsActive);

        }
    }
}