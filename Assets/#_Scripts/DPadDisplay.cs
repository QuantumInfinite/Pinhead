using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DPadDisplay : MonoBehaviour
{
    [Space(10)]

    //REFERENCES TO CANVAS IMAGES
    public GameObject entireDPad;

    [Space(10)]

    //DPad White Arrows
    public Image upArrow;
    public Image downArrow;
    public Image leftArrow;
    public Image rightArrow;

    [Space(10)]

    //DPad Character Heads
    public Image pinHead;
    public Image spindle;
    public Image rebutia;
    public Image clayDoh;

    [Space(10)]

    //GAME EVENT BOOLEANS
    public bool spindleFound = false;
    public bool rebutiaFound = false;
    public bool clayDohFound = false;

    [Space(10)]

    //UI Colors
    private Color transparent;
    private Color opaque;

    [Space(10)]

    //Animator
    public Animator animator;

    [Space(10)]

    //Test Bools
    public bool moving = false;

    void Start()
    {
        //Get transparency color value
        transparent = upArrow.color;
        opaque = upArrow.color;
        opaque.a = 255f;
    }


    void Update()
    {
        if(pinHead.color.a == 1)
        {
            Debug.Log(pinHead.color.a);
            upArrow.gameObject.SetActive(true);
            downArrow.gameObject.SetActive(true);
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
        }

        if (pinHead.color.a < 1)
        {
            Debug.Log(pinHead.color.a);
            upArrow.gameObject.SetActive(false);
            downArrow.gameObject.SetActive(false);
            leftArrow.gameObject.SetActive(false);
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

        //TESTING PURPOSES
        if(moving == false)
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
