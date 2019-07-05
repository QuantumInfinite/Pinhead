using UnityEngine;
using UnityEngine.SceneManagement;

public class EndOfLevelScript : MonoBehaviour
{
    public float delay;
    public string endSceneName;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            Invoke("Transition", delay);
        }
        else
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
        }
    }
    void Transition()
    {
        SceneManager.LoadScene(endSceneName);
    }
}
