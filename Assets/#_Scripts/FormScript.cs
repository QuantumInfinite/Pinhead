using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.VFX;
/*
 * Author: Kyle Jones
 * 
 * Version: 1.5
 * 
 * Description: Allows the player to throw a pin
 * 
 * 
 */
public class FormScript : MonoBehaviour
{
    [Header("Character parts")]
    public GameObject playerRoot; //For spawning
    public GameObject baseModel; //For changing forms
    public GameObject rebutiaRollForm;// For rolling
    public GameObject UpperTorso;
   // public ParticleSystem formChangeParticles;

   [Header("VFX")]
    public VisualEffect characterSwap;
    public Transform VFXSpawnerTransform;
    public GameObject[] HeadPins;

    public GameObject pin; //Pin to throw
    public Animator animator; //Animator
    public SkinnedMeshRenderer materialRenderer; //Where to apply the materials

    [Header("Sounds")]
    public AudioClip pinAimSound;
    public AudioClip pinThrowSound;
    public AudioClip pinPickupSound;
    public ParticleSystem pinPickupEffect;
    public AudioClip pinRecallSound;
    public AudioClip swingStartSound;
    public AudioClip swingReleaseSound;
    public AudioClip formChangeSound;
    public AudioClip pinEmptySound;
    private AudioSource aSource;

    [Header("Pinhead")]
    public Form currentForm = Form.Pin;
    public int pinCount; //Count of available pins
    public float pinThrowTime; //throw time
    public float pinDespawnTimer; //Time for pins to disapear
    public Transform pinSpawnMarker; //where to spawn pin

    [Header("Spindle")]
    public float range;
    public int maxSwingLength = 15;
    public float swingingAirDecel;
    public GameObject yarnSpawnMarker;

    [Header("Clay-doh")]
    public float heavyWeight;
    public float heavySpeed;
    public Color32 clayNormalColor = new Color(1, 1, 1, 1);
    public Color32 clayHeavyColor = new Color(0.5F, 0.5f, 0.5F, 1);

    [Header("General")]
    public Material[] materials;
    public List<GameObject> pinList; //list of all pins
    [HideInInspector]
    public bool abilityIsActive;
    public bool enableAllForms;
    public BoxCollider mainColider;
    public SphereCollider rebutiaColider;

    [Header("Hats")]
    public GameObject pinheadHat;
    public GameObject rebutiaHat;
    public GameObject rebutiaRollingHat;
    public GameObject spindleHat;
    public GameObject claydoughHat;
    public bool hasHatPinhead;
    public bool hasHatRebutia;
    public bool hasHatSpindle;
    public bool hasHatClaydough;


    //Private members
    private float regularWeight;
    private float regularSpeed;

    private Throwing pushPullGrabHandler;

    Rigidbody rigid;

    GameObject pinInstance; //Current Pin

    Vector3 torsoRotation;

    GameObject pivot; //What to move around

    ConfigurableJoint joint; //What to connect to
    bool drawLine = false;
    float airDecel; //used to restore deceleration 

    PlayerMove playerMove;

    private VirtualInputModule virtualInput;
    
    GameObject UIManagaer;

    backgroundAudioMixer bgAudio;

   

    //Enums
    public enum DPadDirection
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    public enum Form
    {
        Regular,
        Yarn,
        Pin,
        Roll,
        Heavy
    }

    private void Start()
    {
        pushPullGrabHandler = GetComponent<Throwing>(); //this is for pickup/push/pull. NOT PINS
        pinList = new List<GameObject>();
        torsoRotation = UpperTorso.transform.localEulerAngles;
        aSource = GetComponent<AudioSource>();
        UIManagaer = GameObject.FindGameObjectWithTag("UI");
        if (enableAllForms && UIManagaer)
        {
            UIManagaer.GetComponent<PauseMenu>().EnableForm(Form.Heavy);
            UIManagaer.GetComponent<PauseMenu>().EnableForm(Form.Yarn);
            UIManagaer.GetComponent<PauseMenu>().EnableForm(Form.Pin);
            UIManagaer.GetComponent<PauseMenu>().EnableForm(Form.Roll);
        }
        rebutiaRollForm.SetActive(false);
        PinsInHead();
        bgAudio = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<backgroundAudioMixer>();
        if (pinheadHat) pinheadHat.SetActive(false);
        if (rebutiaHat) rebutiaHat.SetActive(false);
        if (spindleHat) spindleHat.SetActive(false);
        if (claydoughHat) claydoughHat.SetActive(false);
        playerMove = GetComponent<PlayerMove>();
        rigid = playerRoot.GetComponent<Rigidbody>();
        virtualInput = UIManagaer.GetComponent<VirtualInputModule>();
        regularWeight = rigid.mass;
        regularSpeed = playerMove.maxSpeed;
    }

    private bool test = false;

    void FixedUpdate()
    {
        CheckAbilities(); //Duplicate code because cinimachine dumb
    }
    void Update()
    {
        //Change forms
        DPadDirection dPadInput = GetDPad();
        if (dPadInput != DPadDirection.None)
        {
            ChangeForm(dPadInput);
        }

        CheckAbilities();  //Duplicate code because cinimachine dumb

        if (abilityIsActive)
        { //DoingStuff while active
            switch (currentForm)
            {
                case Form.Pin:
                    //Raycast and aim

                    Vector3 point = FireRay().point;
                    playerRoot.transform.LookAt(new Vector3(point.x, transform.position.y, point.z));
                    UpperTorso.transform.LookAt(point); //have player look at mouse
                    break;

                case Form.Yarn:
                    //Do nothing
                    //Debug.Break();
                    if (Input.GetAxis("Vertical") > 0.1f && test == false)
                    {
                        test = true;
                    }
                    break;
                case Form.Roll:
                    rebutiaRollForm.GetComponent<Animator>().SetBool("Rolling", Math.Abs(Input.GetAxis("Horizontal")) > 0.01f);
                    
                    break;
                case Form.Heavy:
                    //Do nothing
                    break;
            }
        }
        else //Do stuff while ability is not active
        {
            //PinsInHead();
            if (Input.GetButtonDown("Recall"))
            {
                RecallPins();
                PinsInHead();
            }
        }
        if (drawLine)
        {
            //Draw Line
            Vector3[] positions = {yarnSpawnMarker.transform.position, pivot.transform.position };
            GetComponent<LineRenderer>().SetPositions(positions);
        }
    }

    void CheckAbilities()
    {
        if (Input.GetButtonDown("Fire") && !UIManagaer.GetComponent<PauseMenu>().IsPaused)
        {
            ActivateAbility();
        }
        if (Input.GetButtonUp("Fire") && !UIManagaer.GetComponent<PauseMenu>().IsPaused)
        {
            DeactivateAbility();
        }
    }

    //Collisions
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "PinPickup")
        {
            pinCount++;
            //  StartCoroutine(pinEffectShader.StartFresnel());
            other.gameObject.SetActive(false);

            if (pinPickupEffect)
            {
                baseModel.GetComponent<Animator>().SetTrigger("FresnelAnim");
                ParticleSystem.Instantiate(pinPickupEffect, transform.position + new Vector3(0, 3, 0), transform.rotation);


                if (pinPickupSound)
                {
                    aSource.volume = 1;
                    aSource.clip = pinPickupSound;
                    aSource.Play();
                }

                PinsInHead();
            }
        }
    }

    void DeactivateAbility()
    {
        if (abilityIsActive)
        {
            AudioClip clip = null;
            switch (currentForm)
            {
                case Form.Pin:
                    //virtualInput.EnableCursor(false);
                    RaycastHit hit = FireRay();
                    if (hit.transform && (hit.transform.tag == "SidePin" || hit.transform.tag == "BackPin" || hit.transform.tag == "Destroyable") && hit.distance <= range)
                    {
                        ThrowAt(pinInstance, FireRay(), pinThrowTime);

                        //Remove Control of this pin
                        pinInstance = null;

                        //Play throw animation
                        animator.SetTrigger("Throw");

                        //Change sound
                        clip = pinThrowSound;
                    }
                    else
                    {
                        pinCount++;
                        Destroy(pinInstance);
                        animator.SetTrigger("Idle");
                    }
                    PinsInHead();
                    UpperTorso.transform.localEulerAngles = torsoRotation;
                    playerMove.horizontalMovementAllowed = true;
                    Cursor.visible = false;
                    break;
                case Form.Yarn:
                    //body.AddRelativeForce(new Vector3(400, 500));
                    Destroy(joint);

                    animator.SetTrigger("StopSwinging");
                    //Add deceleration back
                    playerMove.airDecel = airDecel;

                    //Stop drawing
                    GetComponent<LineRenderer>().enabled = false;
                    drawLine = false;

                    //change sound
                    clip = swingReleaseSound;
                    break;
                case Form.Roll:
                    //Change form
                    rebutiaRollForm.SetActive(false);
                    baseModel.SetActive(true);

                    mainColider.enabled = true;
                    rebutiaColider.enabled = false;

                    PinsInHead();
                    if (hasHatRebutia)
                    {
                        rebutiaRollingHat.SetActive(false);
                        rebutiaHat.SetActive(true);
                    }
                    break;
                case Form.Heavy:
                    materialRenderer.material.color = clayNormalColor;
                    pushPullGrabHandler.ReleaseGrab();
                    break;
            }
            //play sound
            if (clip)
            {
                aSource.volume = 1;
                aSource.clip = clip;
                aSource.Play();
            }
            //Set inactive
            abilityIsActive = false;
        }

    }

    void ActivateAbility()
    {
        if (abilityIsActive)
        {
            return;
        }
        AudioClip clip = null;
        switch (currentForm)
        {
            case Form.Pin:
                if (pinCount > 0)
                {//Entering Pin form
                 //Set active
                    pinCount--;
                    //virtualInput.EnableCursor(true);
                    abilityIsActive = true;

                    //Set animation
                    animator.SetTrigger("Aim");

                    //Create Pin
                    pinInstance = GameObject.Instantiate(pin, pinSpawnMarker.transform.position, playerRoot.transform.rotation);
                    pinInstance.transform.parent = pinSpawnMarker;

                    //Lock character
                    playerMove.horizontalMovementAllowed = false;
                    //rigid.velocity = Vector3.zero;

                    //Show curser DEPRICATED AS NO LONGER WORKS
                    //Cursor.visible = true;
                    //Cursor.lockState = CursorLockMode.Locked; //This and next line center curser on screen 
                    //Cursor.lockState = CursorLockMode.None;   //^
                    
                    //change sound
                    aSource.volume = 0.5f;
                    clip = pinAimSound;
                }
                else
                {
                    aSource.volume = 1;
                    aSource.clip = pinEmptySound;
                    aSource.Play();
                }
                PinsInHead();
                break;
            case Form.Yarn:

                GameObject nearestPin = GetNearestPin();
                if (nearestPin != null)
                { //Pin found. Connect
                    pivot = nearestPin.GetComponent<PinScript>().pivotPoint;
                  //Set active
                    abilityIsActive = true;

                    animator.SetTrigger("Swinging");
                    AddJoint(pivot);

                    //remove deceleration
                    airDecel = playerMove.airDecel;
                    playerMove.airDecel = swingingAirDecel;

                    //Draw Line
                    GetComponent<LineRenderer>().enabled = true;
                    drawLine = true;

                    //change sound
                    clip = swingStartSound;
                }
                break;
            case Form.Roll:
                //Change form
                abilityIsActive = true;
                rebutiaRollForm.SetActive(true);

                baseModel.SetActive(false);

                //Hide pins
                for (int i = 0; i < HeadPins.Length; i++)
                {
                    HeadPins[i].SetActive(false);
                }

                mainColider.enabled = false;
                rebutiaColider.enabled = true;

                if (hasHatRebutia)
                {
                    rebutiaRollingHat.SetActive(true);
                    rebutiaHat.SetActive(false);
                }
                break;
            case Form.Heavy:
                //Set active
                abilityIsActive = true;
                materialRenderer.material.color = clayHeavyColor;
                pushPullGrabHandler.TryGrab();
                break;
        }
        if (clip)
        {
            aSource.volume = 1;
            aSource.clip = clip;
            aSource.Play();
        }
    }

    public void ChangeForm(DPadDirection dir)
    {
        Form newForm = Form.Regular;
        switch (dir)
        {
            case DPadDirection.None:
                return;
            case DPadDirection.Up:
                newForm = Form.Yarn;
                break;
            case DPadDirection.Down:
                newForm = Form.Heavy;
                break;
            case DPadDirection.Left:
                newForm = Form.Pin;
                break;
            case DPadDirection.Right:
                newForm = Form.Roll;
                break;
            default:
                break;
        }
        ChangeForm(newForm);
    }


    public void ChangeForm(Form switchTo)
    {
        if (switchTo == currentForm || abilityIsActive)
        {
            return;
        }
        //  if (formChangeParticles) {
        //     ParticleSystem.Instantiate(formChangeParticles, transform.position, transform.rotation);
        //  }
        if (characterSwap)
        {
            characterSwap.transform.position =  VFXSpawnerTransform.position;
            characterSwap.SendEvent("PlaySparkles");
            
        }
        if (formChangeSound)
        {
            aSource.volume = 1;
            aSource.clip = formChangeSound;
            aSource.Play();
        }
        if (currentForm == Form.Heavy)
        {
            rigid.mass = regularWeight;
            animator.speed = 0.75f;
            playerMove.maxSpeed = regularSpeed;
        }
        SetHat(switchTo);

        switch (switchTo)
        {
            //PIN
            case Form.Pin:
                //PinsInHead();
                //Change form
                currentForm = Form.Pin;
                characterSwap.SendEvent("toPinhead");
                //change material
                materialRenderer.material = materials[0];
                break;
            //YARN
            case Form.Yarn:
                //Change form
                currentForm = Form.Yarn;
                //change material
                materialRenderer.material = materials[1];
                characterSwap.SendEvent("toSpindle");
                break;
            case Form.Roll:
                //Change form
                currentForm = Form.Roll;
                //change material
                materialRenderer.material = materials[2];
                characterSwap.SendEvent("toRebutia");
                break;
            case Form.Heavy:
                //Change form
                currentForm = Form.Heavy;
                //change material
                materialRenderer.material = materials[3];
                characterSwap.SendEvent("toClaydoh");
                rigid.mass = heavyWeight;
                animator.speed = 0.75f;
                playerMove.maxSpeed = heavySpeed;
                break;
        }
        bgAudio.SetTrack(currentForm);
    }
    public void SetHat(Form form)
    {
        
        if (pinheadHat) pinheadHat.SetActive(false);
        if (claydoughHat) claydoughHat.SetActive(false);
        if (rebutiaHat) rebutiaHat.SetActive(false);
        if (spindleHat) spindleHat.SetActive(false);
        if (rebutiaRollingHat) rebutiaRollingHat.SetActive(false);
        switch (form)
        {
            case Form.Yarn:
                if (hasHatSpindle)
                {
                    if (spindleHat) spindleHat.SetActive(true);
                }
                break;
            case Form.Pin:
                if (hasHatPinhead)
                {
                    if (pinheadHat) pinheadHat.SetActive(true);
                }
                break;
            case Form.Roll:
                if (hasHatRebutia)
                {
                    if (rebutiaHat) rebutiaHat.SetActive(true);
                    if (rebutiaRollingHat) rebutiaRollingHat.SetActive(true);
                }
                break;
            case Form.Heavy:
                if (hasHatClaydough)
                {
                    if (claydoughHat) claydoughHat.SetActive(true);
                }
                break;
        }
    }
    //SWING FUNCTIONS
    GameObject GetNearestPin()
    {
        Vector3 sphereCastStart = transform.position;
        sphereCastStart.z = -maxSwingLength;

        GameObject[] nearbyPins = Physics.SphereCastAll(sphereCastStart, maxSwingLength, new Vector3(0,0,1))
            .Where( x => x.transform && x.transform.tag == "Pin")
            .Select(x => x.transform.gameObject)
            .ToArray();
        GameObject nearestPin = null;
        float pinDistance = Mathf.Infinity;
        //Go through each pin and find their distance
        foreach (GameObject pin in nearbyPins)
        {
            float dist = Vector2.Distance(transform.position, pin.transform.position);
            if (pin.GetComponent<PinScript>()?.currentPinMode == PinScript.PinMode.back && dist < pinDistance && dist < maxSwingLength)
            {
                nearestPin = pin;
                pinDistance = dist;
            }
        }

        return nearestPin;
    }
    void AddJoint(GameObject target)
    {
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = target.GetComponent<Rigidbody>();
        joint.anchor = Vector3.zero;
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
    }

    //PIN FUNCTIONS
    RaycastHit FireRay()
    {
        if (virtualInput != null)
        {
            Ray raymond = Camera.main.ScreenPointToRay(virtualInput.GetVirtualCursorPosition());
            //raymond.origin = new Vector3(raymond.origin.x, raymond.origin.y, -7.5f);
            Debug.DrawRay(raymond.origin, raymond.direction * 100f, Color.red, 2.0f);
            RaycastHit hit;
            Physics.Raycast(raymond, out hit, 1000);
            //GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //s.transform.localScale *= 0.1f;
            //DestroyImmediate(s.GetComponent<Collider>());
            //s.transform.position = hit.point;
            //hit.point = new Vector3(hit.point.x,hit.point.y, 0);
            //print(hit.transform?.name);
            return hit;
        }
        else
        {
            Ray raymond = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(raymond, out hit, 1000);
            //hit.point = new Vector3(hit.point.x,hit.point.y, 0);
            return hit;
        }

    }
    void PinsInHead()
    {
        for (int i = 0; i < HeadPins.Length; i++)
        {
            HeadPins[i].SetActive(i < pinCount);
        }
    }
    void RecallPins()
    {
        pinCount += pinList.Count;
        foreach (GameObject p in pinList)
        {
            Destroy(p);
        }
        pinList.Clear();
        if (pinRecallSound)
        {
            aSource.volume = 1;
            aSource.clip = pinRecallSound;
            aSource.Play();
        }
    }

    void ThrowAt(GameObject Pin, RaycastHit Target, float hitTimer)
    {
        //Add this pin to pinList
        pinList.Add(Pin);

        //setup Object
        Pin.transform.parent = null;
        Pin.AddComponent<Rigidbody>();
        Pin.GetComponent<BoxCollider>().enabled = true;
        Pin.transform.LookAt(Target.point);

        //Do the calculations
        float distanceX = -(Pin.transform.position.x - Target.point.x);
        float distanceY = (Pin.transform.position.y - Target.point.y);
        float distanceZ = (Pin.transform.position.z - Target.point.z);

        //Z distance override for sidePin
        if (Target.transform.tag == "SidePin")
        {
            distanceZ = (Pin.transform.position.z - transform.position.z);
            //Pin.transform.position = new Vector3(Pin.transform.position.x, Pin.transform.position.y, playerRoot.transform.position.z);
        }


        float timeToHit = (Mathf.Abs(distanceX) + Mathf.Abs(distanceZ)) / pinThrowTime;
        float xVol = distanceX / timeToHit;
        float yVol = -(distanceY / timeToHit) - (Physics.gravity.y * timeToHit) / 2;
        float zVol = -(distanceZ / timeToHit);



        //Apply the velocity
        Pin.GetComponent<Rigidbody>().velocity = new Vector3(xVol, yVol, zVol);
        Pin.GetComponent<Rigidbody>().useGravity = true;



        if (Target.transform.tag == "BackPin")//Noramlize to Z
        {
            Pin.GetComponent<PinScript>().NormalizeZ();
        }
        else if (Target.transform.tag == "SidePin")
        {
            Pin.GetComponent<PinScript>().NormalizeX();
        }

        else if (Target.transform.tag == "Destroyable")
        {
            //Do nadda
        }
        else
        {
            Pin.GetComponent<PinScript>().DeleteAfter(pinDespawnTimer);
            Pin.GetComponent<PinScript>().SetColiders(true);
        }

    }
    public DPadDirection GetDPad()
    {
        float hAxis = Input.GetAxis("DPadX");
        float vAxis = Input.GetAxis("DPadY");
        if (Mathf.Abs(hAxis) + Mathf.Abs(vAxis) > 0.1f)
        {
            float hStrength = Mathf.Abs(hAxis);
            float vStrength = Mathf.Abs(vAxis);
            if (hStrength > vStrength)
            {
                return (hAxis < 0) ? DPadDirection.Left : DPadDirection.Right;
            }
            else if (hStrength < vStrength)
            {
                return (vAxis < 0) ? DPadDirection.Down : DPadDirection.Up;
            }
        }
        return DPadDirection.None;
    }
}