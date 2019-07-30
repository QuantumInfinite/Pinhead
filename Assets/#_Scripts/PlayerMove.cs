using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine.Utility;
using UnityEngine;

//handles player movement, utilising the CharacterMotor class
[RequireComponent(typeof(CharacterMotor))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    //movement
    public float accel = 70f; //acceleration/deceleration in air or on the ground

    public float airAccel = 18f;
    public float airDecel = 1.1f;
    private float airPressTime, groundedCount, curAccel, curDecel, curRotateSpeed, slope;

    public Animator animator; //object with animation controller on, which you want to animate
    private AudioSource aSource;

    private CharacterMotor characterMotor;
    public float decel = 7.6f;
    private Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
    private List<Transform> floorCheckers;

    private bool grounded;

    public bool allowUserInput = true;

    public float jumpDelay = 0.1f; //how fast you need to jump after hitting the ground, to do the next type of jump

    //jumping
    public Vector3 jumpForce = new Vector3(0, 13, 0); //normal jump force
    public float jumpLeniancy = 0.17f; //how early before hitting the ground you can press jump, and still have it work
    public AudioClip jumpSound; //play when jumping
    public AudioClip landSound; //play when landing on ground

    public Transform
        mainCam,
        floorChecks; //main camera, and floorChecks object. FloorChecks are raycasted down from to check the player is grounded.

    public float maxSpeed = 9; //maximum speed of movement in X/Z axis

    public float
        movingPlatformFriction =
            7.7f; //you'll need to tweak this to get the player to stay on moving platforms properly

    [HideInInspector] public int onEnemyBounce;

    private PauseMenu pausemenu;

    //private EnemyAI enemyAI;
    //private DealDamage dealDamage;
    private Rigidbody rigid;

    [Range(0f, 5f)]
    public float
        rotateSpeed = 0.7f, airRotateSpeed = 0.4f; //how fast to rotate on the ground, how fast to rotate in the air

    private Quaternion screenMovementSpace;

    //setup
    public bool sidescroller; //if true, won't apply vertical input

    public float
        slopeLimit = 40,
        slideAmount = 35; //maximum angle of slopes you can walk on, how fast to slide down slopes you can't

    public AudioClip walkSound;
    private float zConstant;

    public float rebutiaClimbSpeed = 1.5f;
    //setup
    private void Awake()
    {
        //create single floorcheck in centre of object, if none are assigned
        if (!floorChecks || !floorChecks.gameObject.activeInHierarchy || floorChecks.childCount == 0)
        {
            floorChecks = new GameObject().transform;
            floorChecks.name = "FloorChecks";
            floorChecks.parent = transform;
            floorChecks.position = transform.position;
            var check = new GameObject();
            check.name = "Check1";
            check.transform.parent = floorChecks;
            check.transform.position = transform.position;
            Debug.LogWarning("No 'floorChecks' assigned to PlayerMove script, so a single floorcheck has been created",
                floorChecks);
        }

        //assign player tag if not already
        if (tag != "Player")
        {
            tag = "Player";
            Debug.LogWarning(
                "PlayerMove script assigned to object without the tag 'Player', tag has been assigned automatically",
                transform);
        }

        //usual setup
        mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
        //dealDamage = GetComponent<DealDamage>();
        characterMotor = GetComponent<CharacterMotor>();
        rigid = GetComponent<Rigidbody>();
        aSource = GetComponent<AudioSource>();


        zConstant = rigid.position.z;

        //gets child objects of floorcheckers, and puts them in an array
        //later these are used to raycast downward and see if we are on the ground
        floorCheckers = new List<Transform>();
        //floorCheckers = new Transform[floorChecks.childCount];
        for (var i = 0; i < floorChecks.childCount; i++)
        {
            if (floorChecks.GetChild(i).gameObject.activeInHierarchy)
            {
                floorCheckers.Add(floorChecks.GetChild(i));
            }
        }
    }

    private void Start()
    {
        var ui = GameObject.FindGameObjectWithTag("UI");
        if (ui)
        {
            pausemenu = ui.GetComponent<PauseMenu>();
        }
        else
        {
            Debug.LogWarning("No game object tagged UI, pause functions will not work");
        }
    }

    //get state of player, values and input
    private void Update()
    {
        //stops rigidbody "sleeping" if we don't move, which would stop collision detection
        rigid.WakeUp();
        //handle jumping
        if (allowUserInput)
        {
            JumpCalculations();
        }

        //adjust movement values if we're in the air or on the ground
        curAccel = grounded ? accel : airAccel;
        curDecel = grounded ? decel : airDecel;
        curRotateSpeed = grounded ? rotateSpeed : airRotateSpeed;

        //get movement axis relative to camera
        screenMovementSpace = Quaternion.Euler(0, mainCam.eulerAngles.y, 0);
        screenMovementForward = screenMovementSpace * Vector3.forward;
        screenMovementRight = screenMovementSpace * Vector3.right;

        //get movement input, set direction to move in
        float h = 0;
        float v = 0;
        if (allowUserInput)
        {
            v = Input.GetAxisRaw("Vertical");
            h = Input.GetAxisRaw("Horizontal");
        }

        //only apply vertical input to movemement, if player is not sidescroller
        if (!sidescroller)
        {
            direction = screenMovementForward * v + screenMovementRight * h;
        }
        else
        {
            direction = Vector3.right * h;
        }

        moveDirection = transform.position + direction;
    }

    //apply correct player movement (fixedUpdate for physics calculations)
    private void FixedUpdate()
    {
        //are we grounded
        grounded = IsGrounded();

        //move, rotate, manage speed
        characterMotor.MoveTo(moveDirection, curAccel, 0.7f, true);
        if (Math.Abs(rotateSpeed) > 0.01f && Math.Abs(direction.magnitude) > 0.01f)
        {
            characterMotor.RotateToDirection(moveDirection, curRotateSpeed * 5, true);
        }

        characterMotor.ManageSpeed(curDecel, maxSpeed + movingObjSpeed.magnitude, true);
        //Movement sounds
        if (grounded && Math.Abs(Input.GetAxis("Horizontal")) > 0.1f && walkSound && aSource.clip != walkSound)
        {
            aSource.volume = 1;
            aSource.clip = walkSound;
            aSource.Play();
        }

        //set animation values
        if (animator && animator.isActiveAndEnabled)
        {
            animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
            animator.SetBool("Grounded", grounded);
            animator.SetFloat("YVelocity", rigid.velocity.y);
            if (!grounded && rigid.velocity.y < -0.01)
            {
                var ray = new Ray(floorCheckers[0].position, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    //print("Max point at " + hit.distance + " (" + hit.transform.name + ")");
                }
            }
        }

        if (Math.Abs(rigid.position.z - zConstant) > 0.1f)
        {
            rigid.position = new Vector3(rigid.position.x, rigid.position.y, zConstant);
        }

        //Make player come to a hard stop when grounded and not pressing a button. also disable gravity
        if (grounded && Math.Abs(Input.GetAxis("Horizontal")) < 0.1f)
        {
            //rigid.velocity = new Vector3(0, rigid.velocity.y);
            //rigid.useGravity = false;
        }
        else
        {
            ///rigid.useGravity = true;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.collider.tag == "Scalable" && GetComponent<FormScript>().currentForm == FormScript.Form.Roll &&
            GetComponent<FormScript>().abilityIsActive)
        {
            rigid.velocity = Vector3.zero;
        }
    }
    void PrintDir(Collision other)
    {
        Vector3[] points = other.contacts.Select(x => x.point).ToArray();
        foreach (var variable in points)
        {
            Debug.DrawLine(transform.position, variable, Color.red, .2f);
        }
        Vector3 avg = new Vector3(points.Average(x => x.x), points.Average(x => x.y), points.Average(x => x.z));
        print(points.Max(x => x.y) - transform.position.y);
        //Debug.DrawLine(transform.position, avg, Color.red, 2f);
        //print( transform.InverseTransformPoint(avg));
        //print(Mathf.DeltaAngle(transform.position.y, avg.y));
        //print(transform.position - avg);
    }

    bool ShouldClimb(Vector3[] points)
    {
        return (points.Max(x => x.y) > transform.position.y);
    }

    Vector3 AveragePoint(Vector3[] points)
    {
        return new Vector3(points.Average(x => x.x), points.Average(x => x.y), points.Average(x => x.z));
    }

    private GameObject IntersectPoint;
    private void RebutiaClimb(Collision other)
    {
        if (!IntersectPoint)
        {
            IntersectPoint = new GameObject("CLIMB INTERSECT");
        }
        
        rigid.AddForce(-Physics.gravity, ForceMode.Acceleration);
        if (Input.GetButton("Horizontal"))
        {
            Vector3[] points = other.contacts.Select(x => x.point).ToArray();
            Vector3 avgPoint = AveragePoint(points);
            IntersectPoint.transform.position = avgPoint;
            Vector3 cross = Vector3.Cross(transform.position, avgPoint);
            //PrintDir(other);

            Vector3 scaleDir;
            if (avgPoint.x < transform.position.x) //Wall is to the left
            {
                scaleDir = Input.GetAxis("Horizontal") < 0 ? Vector3.up : Vector3.down;
            }
            else //wall is to the right
            {
                scaleDir = Input.GetAxis("Horizontal") < 0 ? Vector3.down : Vector3.up;
            }
            //Only apply if we are not on top of the object
            if (ShouldClimb(points))
            {
                rigid.AddForce(scaleDir * rebutiaClimbSpeed, ForceMode.Acceleration);
                rigid.velocity = Vector3.ClampMagnitude(rigid.velocity, 1);
            }
        }
        else
        {
            rigid.velocity = Vector3.zero;
        }
    }

    //prevents rigidbody from sliding down slight slopes (read notes in characterMotor class for more info on friction)
    private void OnCollisionStay(Collision other)
    {
        if (other.collider.tag == "Scalable" && GetComponent<FormScript>().currentForm == FormScript.Form.Roll && GetComponent<FormScript>().abilityIsActive)
        {
            RebutiaClimb(other);
        }

        //only stop movement on slight slopes if we aren't being touched by anything else
        else if (other.collider.tag != "Untagged" || grounded == false)
        {
            return;
        }
        //if no movement should be happening, stop player moving in Z/X axis
        else if (Math.Abs(direction.magnitude) < 0.01f && Mathf.Abs(slope) > 0.1 && slope < slopeLimit && rigid.velocity.magnitude < 2)
        {
            //it's usually not a good idea to alter a rigidbodies velocity every frame
            //but this is the cleanest way i could think of, and we have a lot of checks beforehand, so it should be ok
            rigid.velocity = Vector3.zero;
            ///rigid.useGravity = false;
        }
    }

    //returns whether we are on the ground or not
    //also: bouncing on enemies, keeping player on moving platforms and slope checking
    private bool IsGrounded()
    {
        //get distance to ground, from centre of collider (where floorcheckers should be)
        var dist = GetComponent<Collider>().bounds.extents.y;
        //check whats at players feet, at each floorcheckers position
        foreach (var check in floorCheckers)
        {
            RaycastHit hit;
            if (Physics.Raycast(check.position, Vector3.down, out hit, dist + 0.1f))
            {
                if (!hit.transform.GetComponent<Collider>().isTrigger)
                {
                    //slope control
                    slope = Vector3.Angle(hit.normal, Vector3.up);


                    //slide down slopes
                    if (slope > slopeLimit && hit.transform.tag != "Pushable")
                    {
                        var slide = new Vector3(0f, -slideAmount, 0f);
                        rigid.AddForce(slide, ForceMode.Force);
                    }

                    //enemy bouncing
                    if (hit.transform.tag == "Enemy" && rigid.velocity.y < 0)
                    {
                        //enemyAI = hit.transform.GetComponent<EnemyAI>();
                        //enemyAI.BouncedOn();
                        onEnemyBounce++;
                        //dealDamage.Attack(hit.transform.gameObject, 1, 0f, 0f);
                    }
                    else
                    {
                        onEnemyBounce = 0;
                    }

                    //moving platforms
                    if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
                    {
                        movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
                        movingObjSpeed.y = 0f;
                        //9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
                        rigid.AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime,
                            ForceMode.VelocityChange);
                    }
                    else
                    {
                        movingObjSpeed = Vector3.zero;
                    }
                    //yes our feet are on something
                    return true;
                }
            }
        }

        movingObjSpeed = Vector3.zero;
        //no none of the floorchecks hit anything, we must be in the air (or water)
        return false;
    }

    //jumping
    private void JumpCalculations()
    {
        //keep how long we have been on the ground
        groundedCount = grounded ? groundedCount += Time.deltaTime : 0f;


        //play landing sound
        if (groundedCount < 0.25 && groundedCount != 0 && !GetComponent<AudioSource>().isPlaying && landSound &&
            rigid.velocity.y < 1)
        {
            aSource.volume = Mathf.Abs(rigid.velocity.y) / 15;
            aSource.clip = landSound;
            aSource.Play();
        }

        //if we press jump in the air, save the time
        if (Input.GetButtonDown("Jump") && !grounded)
        {
            airPressTime = Time.time;
        }

        //if were on ground within slope limit
        if (grounded && slope < slopeLimit)
        {
            //and we press jump, or we pressed jump justt before hitting the ground
            if (Input.GetButtonDown("Jump") || airPressTime + jumpLeniancy > Time.time)
            {
                if (groundedCount > jumpDelay)
                {
                    Jump(jumpForce);
                }
            }
        }
    }

    //push player at jump force
    public void Jump(Vector3 jumpVelocity)
    {
        if (pausemenu && pausemenu.IsPaused)
        {
            return;
        }

        if (jumpSound)
        {
            aSource.volume = 1;
            aSource.clip = jumpSound;
            aSource.Play();
        }

        rigid.velocity = new Vector3(rigid.velocity.x, 0f, 0f);
        rigid.AddRelativeForce(jumpVelocity, ForceMode.Impulse);
        airPressTime = 0f;
    }
}
/* ORIG
 * using UnityEngine;
using System.Collections;

//handles player movement, utilising the CharacterMotor class
[RequireComponent(typeof(CharacterMotor))]
//[RequireComponent(typeof(DealDamage))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour 
{
	//setup
	public bool sidescroller;					//if true, won't apply vertical input
	public Transform mainCam, floorChecks;		//main camera, and floorChecks object. FloorChecks are raycasted down from to check the player is grounded.
	public Animator animator;					//object with animation controller on, which you want to animate
	public AudioClip jumpSound;					//play when jumping
	public AudioClip landSound;					//play when landing on ground
	
	//movement
	public float accel = 70f;					//acceleration/deceleration in air or on the ground
	public float airAccel = 18f;			
	public float decel = 7.6f;
	public float airDecel = 1.1f;
	[Range(0f, 5f)]
	public float rotateSpeed = 0.7f, airRotateSpeed = 0.4f;	//how fast to rotate on the ground, how fast to rotate in the air
	public float maxSpeed = 9;								//maximum speed of movement in X/Z axis
	public float slopeLimit = 40, slideAmount = 35;			//maximum angle of slopes you can walk on, how fast to slide down slopes you can't
	public float movingPlatformFriction = 7.7f;				//you'll need to tweak this to get the player to stay on moving platforms properly
	
	//jumping
	public Vector3 jumpForce =  new Vector3(0, 13, 0);		//normal jump force
	public Vector3 secondJumpForce = new Vector3(0, 13, 0); //the force of a 2nd consecutive jump
	public Vector3 thirdJumpForce = new Vector3(0, 13, 0);	//the force of a 3rd consecutive jump
	public float jumpDelay = 0.1f;							//how fast you need to jump after hitting the ground, to do the next type of jump
	public float jumpLeniancy = 0.17f;						//how early before hitting the ground you can press jump, and still have it work
	[HideInInspector]
	public int onEnemyBounce;					
	
	private int onJump;
	private bool grounded;
	private Transform[] floorCheckers;
	private Quaternion screenMovementSpace;
	private float airPressTime, groundedCount, curAccel, curDecel, curRotateSpeed, slope;
	private Vector3 direction, moveDirection, screenMovementForward, screenMovementRight, movingObjSpeed;
	
	private CharacterMotor characterMotor;
	private EnemyAI enemyAI;
	//private DealDamage dealDamage;
	private Rigidbody rigid;
	private AudioSource aSource;

	//setup
	void Awake()
	{
		//create single floorcheck in centre of object, if none are assigned
		if(!floorChecks)
		{
			floorChecks = new GameObject().transform;
			floorChecks.name = "FloorChecks";
			floorChecks.parent = transform;
			floorChecks.position = transform.position;
			GameObject check = new GameObject();
			check.name = "Check1";
			check.transform.parent = floorChecks;
			check.transform.position = transform.position;
			Debug.LogWarning("No 'floorChecks' assigned to PlayerMove script, so a single floorcheck has been created", floorChecks);
		}
		//assign player tag if not already
		if(tag != "Player")
		{
			tag = "Player";
			Debug.LogWarning ("PlayerMove script assigned to object without the tag 'Player', tag has been assigned automatically", transform);
		}
		//usual setup
		mainCam = GameObject.FindGameObjectWithTag("MainCamera").transform;
		//dealDamage = GetComponent<DealDamage>();
		characterMotor = GetComponent<CharacterMotor>();
		rigid = GetComponent<Rigidbody>();
		aSource = GetComponent<AudioSource>();
		//gets child objects of floorcheckers, and puts them in an array
		//later these are used to raycast downward and see if we are on the ground
		floorCheckers = new Transform[floorChecks.childCount];
		for (int i=0; i < floorCheckers.Length; i++)
			floorCheckers[i] = floorChecks.GetChild(i);
	}
	
	//get state of player, values and input
	void Update()
	{	
		//stops rigidbody "sleeping" if we don't move, which would stop collision detection
		rigid.WakeUp();
		//handle jumping
		JumpCalculations ();
		//adjust movement values if we're in the air or on the ground
		curAccel = (grounded) ? accel : airAccel;
		curDecel = (grounded) ? decel : airDecel;
		curRotateSpeed = (grounded) ? rotateSpeed : airRotateSpeed;
				
		//get movement axis relative to camera
		screenMovementSpace = Quaternion.Euler (0, mainCam.eulerAngles.y, 0);
		screenMovementForward = screenMovementSpace * Vector3.forward;
		screenMovementRight = screenMovementSpace * Vector3.right;
		
		//get movement input, set direction to move in
		float h = Input.GetAxisRaw ("Horizontal");
		float v = Input.GetAxisRaw ("Vertical");
		
		//only apply vertical input to movemement, if player is not sidescroller
		if(!sidescroller)
			direction = (screenMovementForward * v) + (screenMovementRight * h);
		else
			direction = Vector3.right * h;
		moveDirection = transform.position + direction;
	}
	
	//apply correct player movement (fixedUpdate for physics calculations)
	void FixedUpdate() 
	{
		//are we grounded
		grounded = IsGrounded ();
		//move, rotate, manage speed
		characterMotor.MoveTo (moveDirection, curAccel, 0.7f, true);
		if (rotateSpeed != 0 && direction.magnitude != 0)
			characterMotor.RotateToDirection (moveDirection , curRotateSpeed * 5, true);
		characterMotor.ManageSpeed (curDecel, maxSpeed + movingObjSpeed.magnitude, true);
		//set animation values
		if(animator)
		{
			animator.SetFloat("DistanceToTarget", characterMotor.DistanceToTarget);
			animator.SetBool("Grounded", grounded);
			animator.SetFloat("YVelocity", GetComponent<Rigidbody>().velocity.y);
		}
	}
	
	//prevents rigidbody from sliding down slight slopes (read notes in characterMotor class for more info on friction)
	void OnCollisionStay(Collision other)
	{
		//only stop movement on slight slopes if we aren't being touched by anything else
		if (other.collider.tag != "Untagged" || grounded == false)
			return;
		//if no movement should be happening, stop player moving in Z/X axis
		if(direction.magnitude == 0 && slope < slopeLimit && rigid.velocity.magnitude < 2)
		{
			//it's usually not a good idea to alter a rigidbodies velocity every frame
			//but this is the cleanest way i could think of, and we have a lot of checks beforehand, so it should be ok
			rigid.velocity = Vector3.zero;
		}
	}
	
	//returns whether we are on the ground or not
	//also: bouncing on enemies, keeping player on moving platforms and slope checking
	private bool IsGrounded() 
	{
		//get distance to ground, from centre of collider (where floorcheckers should be)
		float dist = GetComponent<Collider>().bounds.extents.y;
		//check whats at players feet, at each floorcheckers position
		foreach (Transform check in floorCheckers)
		{
			RaycastHit hit;
			if(Physics.Raycast(check.position, Vector3.down, out hit, dist + 0.05f))
			{
				if(!hit.transform.GetComponent<Collider>().isTrigger)
				{
					//slope control
					slope = Vector3.Angle (hit.normal, Vector3.up);
					//slide down slopes
					if(slope > slopeLimit && hit.transform.tag != "Pushable")
					{
						Vector3 slide = new Vector3(0f, -slideAmount, 0f);
						rigid.AddForce (slide, ForceMode.Force);
					}
					//enemy bouncing
					if (hit.transform.tag == "Enemy" && rigid.velocity.y < 0)
					{
						enemyAI = hit.transform.GetComponent<EnemyAI>();
						enemyAI.BouncedOn();
						onEnemyBounce ++;
						//dealDamage.Attack(hit.transform.gameObject, 1, 0f, 0f);
					}
					else
						onEnemyBounce = 0;
					//moving platforms
					if (hit.transform.tag == "MovingPlatform" || hit.transform.tag == "Pushable")
					{
						movingObjSpeed = hit.transform.GetComponent<Rigidbody>().velocity;
						movingObjSpeed.y = 0f;
						//9.5f is a magic number, if youre not moving properly on platforms, experiment with this number
						rigid.AddForce(movingObjSpeed * movingPlatformFriction * Time.fixedDeltaTime, ForceMode.VelocityChange);
					}
					else
					{
						movingObjSpeed = Vector3.zero;
					}
					//yes our feet are on something
					return true;
				}
			}
		}
		movingObjSpeed = Vector3.zero;
		//no none of the floorchecks hit anything, we must be in the air (or water)
		return false;
	}
	
	//jumping
	private void JumpCalculations()
	{
		//keep how long we have been on the ground
		groundedCount = (grounded) ? groundedCount += Time.deltaTime : 0f;
		
		//play landing sound
		if(groundedCount < 0.25 && groundedCount != 0 && !GetComponent<AudioSource>().isPlaying && landSound && GetComponent<Rigidbody>().velocity.y < 1)
		{
			aSource.volume = Mathf.Abs(GetComponent<Rigidbody>().velocity.y)/40;
			aSource.clip = landSound;
			aSource.Play ();
		}
		//if we press jump in the air, save the time
		if (Input.GetButtonDown ("Jump") && !grounded)
			airPressTime = Time.time;
		
		//if were on ground within slope limit
		if (grounded && slope < slopeLimit)
		{
			//and we press jump, or we pressed jump justt before hitting the ground
			if (Input.GetButtonDown ("Jump") || airPressTime + jumpLeniancy > Time.time)
			{	
				//increment our jump type if we haven't been on the ground for long
				onJump = (groundedCount < jumpDelay) ? Mathf.Min(2, onJump + 1) : 0;
				//execute the correct jump (like in mario64, jumping 3 times quickly will do higher jumps)
				if (onJump == 0)
						Jump (jumpForce);
				else if (onJump == 1)
						Jump (secondJumpForce);
				else if (onJump == 2){
						Jump (thirdJumpForce);
						onJump --;
				}
			}
		}
	}
	
	//push player at jump force
	public void Jump(Vector3 jumpVelocity)
	{
		if(jumpSound)
		{
			aSource.volume = 1;
			aSource.clip = jumpSound;
			aSource.Play ();
		}
		rigid.velocity = new Vector3(rigid.velocity.x, 0f, rigid.velocity.z);
		rigid.AddRelativeForce (jumpVelocity, ForceMode.Impulse);
		airPressTime = 0f;
	}
}
*/