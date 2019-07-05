using UnityEngine;

public class IgnorePlayerCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }
}
