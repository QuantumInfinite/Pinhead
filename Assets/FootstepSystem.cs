using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class FootstepSystem : MonoBehaviour
{
    private AudioSource audioSource;
    public string currentFloor = "Wood";


    [Header("Wood Footsteps")]
    public AudioClip[] steps;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayFootstep()
    {
            if (currentFloor == "Wood")
            {
                int choice = Random.Range(0, steps.Length - 1);
                Debug.Log("Sound " + choice + " was played.");
                audioSource.clip = steps[choice];
                audioSource.Play();
            }
      
    }
}
