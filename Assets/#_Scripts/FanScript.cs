using UnityEngine;

//add this to your "level goal" trigger
[RequireComponent(typeof(CapsuleCollider))]

public class FanScript : MonoBehaviour
{
    public float lift;              //the lifting force applied to player when theyre inside the goal
    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    //lift player upwards
    void OnTriggerStay(Collider other)
    {
        Rigidbody rigid = other.GetComponent<Rigidbody>();
        if (rigid)
        {
            rigid.AddForce(transform.up * lift, ForceMode.Force);
        }
    }
}