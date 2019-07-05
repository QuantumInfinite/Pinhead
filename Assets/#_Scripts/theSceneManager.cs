using UnityEngine;
using UnityEngine.SceneManagement;

public class theSceneManager : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void StartButton()
    {
        SceneManager.LoadScene(1);
    }

    public void RestartButton()
    {
        SceneManager.LoadScene(0);
    }

    public void QuitButton()
    {
        Debug.Log("Game has quit family");
        Application.Quit();
    }

}
