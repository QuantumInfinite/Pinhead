using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CinematicStart : MonoBehaviour
{
    [SerializeField]
    public PlayableDirector PlayableDirector;
    public string TagName = "Player";

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(TagName))
        {
            PlayableDirector.Play();
        }
    }
}
