using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DPadDisplay : MonoBehaviour
{
    [Header("Player References")]

    //REFERENCE TO THE CHARACTERMOTOR
    public CharacterMotor characterMotor;

    [Header("UI Images")]

    //REFERENCES TO CANVAS IMAGES
    public GameObject entireDPad;

    //DPad White Arrows
    [Space(10)]
    public Image upArrow;
    public Image downArrow;
    public Image leftArrow;
    public Image rightArrow;

    //Dpad White Arrows (Animated)
    [Space(10)]
    public Image upArrowAnim;
    public Image downArrowAnim;
    public Image leftArrowAnim;
    public Image rightArrowAnim;

    //DPad Character Heads
    [Space(10)]
    public Image pinHead;
    public Image spindle;
    public Image rebutia;
    public Image clayDoh;

    [Header("Game Events")]
    //GAME EVENT BOOLEANS
    public bool spindleFound = false;
    public bool rebutiaFound = false;
    public bool clayDohFound = false;

    [Header("Animations")]

    //Animator
    public Animator animator;

    //Test Bools
    public bool moving = false;

    //UI Colors
    private Color transparent;
    private Color opaque;

    void Start()
    {
        //Get transparency color value
        transparent = upArrow.color;
        opaque = upArrow.color;
        opaque.a = 255f;
    }


    void Update()
    {
        //Handles Seperation of Animated Arrows and Input Feedback Arrows
        if(pinHead.color.a == 1)
        {
            upArrowAnim.gameObject.SetActive(false);
            upArrow.gameObject.SetActive(true);
            downArrowAnim.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(true);
            leftArrowAnim.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(true);
            rightArrowAnim.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(true);
        }

        if (pinHead.color.a < 1)
        {
            upArrowAnim.gameObject.SetActive(true);
            upArrow.gameObject.SetActive(false);
            downArrowAnim.gameObject.SetActive(true);
            downArrow.gameObject.SetActive(false);
            leftArrowAnim.gameObject.SetActive(true);
            leftArrow.gameObject.SetActive(false);
            rightArrowAnim.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(false);
        }

        //Up Arrow Input
        if (Input.GetKeyDown("up"))
        {
            upArrow.color = opaque;
        }
        if (Input.GetKeyUp("up"))
        {
            upArrow.color = transparent;
        }

        //Down Arrow Input
        if (Input.GetKeyDown("down"))
        {
            downArrow.color = opaque;
        }
        if (Input.GetKeyUp("down"))
        {
            downArrow.color = transparent;
        }

        //Left Arrow Input
        if (Input.GetKeyDown("left"))
        {
            leftArrow.color = opaque;
        }
        if (Input.GetKeyUp("left"))
        {
            leftArrow.color = transparent;
        }

        //Right Arrow Input
        if (Input.GetKeyDown("right"))
        {
            rightArrow.color = opaque;
        }
        if (Input.GetKeyUp("right"))
        {
            rightArrow.color = transparent;
        }

        //Fading Animation
        if (characterMotor.currentSpeed != Vector3.zero)
        {
            moving = true;
        }

        if (characterMotor.currentSpeed == Vector3.zero)
        {
            moving = false;
        }

        if (moving == false)
        {
            animator.SetBool("MovingStart", false);
            animator.SetBool("MovingStop", true);
            
        }
        if (moving == true)
        {
            animator.SetBool("MovingStop", false);
            animator.SetBool("MovingStart", true);
            
        }
    }
}
