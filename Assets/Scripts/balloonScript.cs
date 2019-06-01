using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class balloonScript : MonoBehaviour {
    AudioSource sound;
    private void Start()
    {
        sound = GetComponent<AudioSource>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Pin")
        {
            Pop();
        }
    }
    void Pop()
    {
        ParticleSystem s = GetComponent<ParticleSystem>();
        s.Play();
        sound.Play();
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;

        Destroy(transform.GetChild(0).gameObject);
        Destroy(gameObject, 1f);
    }
}
