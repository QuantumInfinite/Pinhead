using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newCharacterController : MonoBehaviour {
    CharacterController player;

    [SerializeField]
    float baseSpeed = 6.0f;

    [SerializeField]
    float jumpSpeed = 8.0f;

    [SerializeField]
    float gravity = 9.8f;
    

    Vector3 moveDirection = Vector3.zero;

    bool isSprinting = false;

    void Awake() {
        player = GetComponent<CharacterController>();
    }

    void Update() {

        //Base movemenet
        moveDirection.x = Input.GetAxis("Horizontal") * baseSpeed;
        

        //Jumping
        if (player.isGrounded && Input.GetButton("Jump")) {
            moveDirection.y = jumpSpeed;
        }

        moveDirection.y -= (gravity * Time.deltaTime);

        player.Move(moveDirection * Time.deltaTime);
    }

    //This allows for the player to snap to another position
    public void MovePosition(Vector3 location) {
        player.enabled = false;
        transform.position = location;
        player.enabled = true;
    }
}
