using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PauseMenu : MonoBehaviour {

    public GameObject PauseUI;
    public GameObject RadialUI;
    public bool IsPaused = false;
    GameObject player;
    enum Menu
    {
        Pause,
        Radial,
        None
    }
    Menu currentMenu;

    public GameObject[] formButtons;
    // Use this for initialization
    void Start () {
        PauseUI.SetActive(false);
        RadialUI.SetActive(false);
        foreach (Image img in RadialUI.GetComponentsInChildren<Image>())
        {
            img.alphaHitTestMinimumThreshold = 0.5f;
        }
        currentMenu = Menu.None;
        player = GameObject.FindGameObjectWithTag("Player");
    }
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("escape") && currentMenu != Menu.Radial)
        {
            currentMenu = Menu.Pause;
            TogglePause(PauseUI);

        }
        if (Input.GetMouseButtonDown(1) && currentMenu != Menu.Pause && !player.GetComponent<FormScript>().abilityIsActive) 
        {
            currentMenu = Menu.Radial;
            TogglePause(RadialUI);
        }
	}
    
    public void EnableForm(FormScript.Form form)
    {
        int val = 0;
        switch (form)
        {
            case FormScript.Form.Roll:
                val = 0;
                break;
            case FormScript.Form.Pin:
                val = 1;
                break;
            case FormScript.Form.Heavy:
                val = 2;
                break;
            case FormScript.Form.Yarn:
                val = 3;
                break;
        }
        formButtons[val].GetComponent<Button>().enabled = true;
    }

    public void TogglePause(GameObject UIobject)
    {
        Time.timeScale = Mathf.Abs(Time.timeScale - 1f);

        UIobject.SetActive(!UIobject.activeInHierarchy);
        IsPaused = !IsPaused;
        
        if (IsPaused)
        {
            //Show curser
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Locked; //This and next line center curser on screen 
            Cursor.lockState = CursorLockMode.None;   //^
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked; //This and next line center curser on screen 
            Cursor.lockState = CursorLockMode.None;   //^
            currentMenu = Menu.None;
        }

    }
    public void MenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void ResumeButton()
    {
        TogglePause(PauseUI);
    }

    public void SetFormRoll()
    {
        player.GetComponent<FormScript>().ChangeForm(FormScript.Form.Roll);
        TogglePause(RadialUI);
    }
    public void SetFormPin()
    {
        player.GetComponent<FormScript>().ChangeForm(FormScript.Form.Pin);
        TogglePause(RadialUI);
    }
    public void SetFormHeavy()
    {
        player.GetComponent<FormScript>().ChangeForm(FormScript.Form.Heavy);
        TogglePause(RadialUI);
    }
    public void SetFormYarn()
    {
        player.GetComponent<FormScript>().ChangeForm(FormScript.Form.Yarn);
        TogglePause(RadialUI);
    }

}
