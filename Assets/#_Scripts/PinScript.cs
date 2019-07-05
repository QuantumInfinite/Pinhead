using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
/*
 * Author: Kyle Jones
 * 
 * Version: 1.2
 * 
 * Description: Controls basic functionality of pins
 * 
 * 
 */
public class PinScript : MonoBehaviour {
    public GameObject pivotPoint;
    public enum PinMode
    {
        side,
        back
    }
    public PinMode currentPinMode;
    public float turnSpeed;

    public enum Direction
    {
        left,
        right
    }
    Direction currentDir;


    public float TimeSinceRotationStart = 0f;
    public float RotationTime; // Take 1 second to rotate fully

    public Vector3 CurrentLerpValue;
    Vector3 finish;
    Vector3 start;
    public VisualEffect pinImpact;
    public AudioClip pinStickSound;
    public AudioClip pinImpactSound;
    AudioSource aSource;
    // Use this for initialization
    void Start () {
        aSource = GetComponent<AudioSource>();
        SetColiders(false);
    }
	// Update is called once per frame
	void Update () {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "BackPin")
        {
            transform.localEulerAngles = new Vector3(0, 0, 0);
            //Destroy this rigidbody
            Destroy(GetComponent<Rigidbody>());

            //Add Pivot point for swing
            pivotPoint.AddComponent<Rigidbody>();
            pivotPoint.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            pivotPoint.GetComponent<Rigidbody>().useGravity = false;
            pivotPoint.GetComponent<Rigidbody>().mass = 2000;
            pivotPoint.GetComponent<Rigidbody>().angularDrag = 0;

            currentPinMode = PinMode.back;
            if (pinImpact)
            {
                pinImpact.SendEvent("PinImpact");
            }
            //play sound
            if (pinStickSound)
            {
                aSource.volume = 0.25f;
                aSource.clip = pinStickSound;
                aSource.Play();

            }
        }
        else if (collision.gameObject.tag == "SidePin")
        {
            transform.localEulerAngles = finish;
            SetColiders(true);
            //Destroy this rigidbody to stop falling
            Destroy(GetComponent<Rigidbody>());
            currentPinMode = PinMode.side;
            if (pinImpact)
            {
                pinImpact.SendEvent("PinImpact");
            }

            if (pinStickSound)
            {
                aSource.volume = 0.25f;
                aSource.clip = pinStickSound;
                aSource.Play();
            }
        }
        else if (collision.gameObject.tag == "Destroyable")
        {
            SetColiders(true);
            if (pinImpact)
            {
                pinImpact.SendEvent("PinImpact");
            }
            if (pinImpactSound)
            {
                aSource.volume = 1;
                aSource.clip = pinImpactSound;
                aSource.Play();
            }
        }
    }

    public void SetColiders(bool b)
    {
        foreach (Transform child in transform)
        {
            BoxCollider c = child.GetComponent<BoxCollider>();
            if (c)
            {
                c.enabled = b;
            }

        }
    }

    public void NormalizeZ() {
        finish = new Vector3(0, 0, 0);
        StartCoroutine(RotateToRotation(transform, finish, RotationTime));
    }
    public void NormalizeX()
    {
        if (GetComponent<Rigidbody>().velocity.x > 0)
        {
            finish = new Vector3(360, 90, 0);
        }
        else
        {
            finish = new Vector3(360, 270, 0);
        }
        StartCoroutine(RotateToRotation(transform, finish, RotationTime));

    }
    public void DeleteAfter(float f)
    {
        StartCoroutine(DeleteThis(f));
    }

    IEnumerator DeleteThis(float f)
    {
        yield return new WaitForSeconds(f);
        GameObject.FindGameObjectWithTag("Player").GetComponent<FormScript>().pinCount++;
        GameObject.FindGameObjectWithTag("Player").GetComponent<FormScript>().pinList.Remove(this.gameObject);
        Destroy(this.gameObject);
    }

    IEnumerator RotateToRotation(Transform transform, Vector3 final, float timeToMove)
    {
        var start = transform.localEulerAngles;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / timeToMove;

            transform.localEulerAngles = new Vector3(Mathf.LerpAngle(start.x, final.x, t), Mathf.LerpAngle(start.y, final.y, t), Mathf.LerpAngle(start.z, final.z, t));
            //Debug.Break();
            yield return null;
        }
    }
}
