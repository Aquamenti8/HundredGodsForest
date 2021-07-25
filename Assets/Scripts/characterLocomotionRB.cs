using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class characterLocomotionRB : MonoBehaviour
{
    //Third Person Basic Move
    public Transform cam;
    Animator animator;
    Rigidbody rb;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocityGrounded;
    float turnSmoothVelicityHanging;

    public float gravity = -9.81f;

    //STATES
    public enum State { OnGround, OnJump, OnAir, OnWall, OnAttractUse}
    public State state;

    //ANIMATION
    public float animSmoothTime = 0.05f;
    Vector3 animSmoothVelocity;
    Vector3 animMovement;
    float timeSinceInput;
    public float timeBeforeRest = 2f;
    float jumpAnimDuration = 0.3f;

    bool isGrounded;
    bool isWall;
    bool isWallUp;
    bool isFalling;
    bool isHanging;
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform wallUpCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    // INPUT
    Vector3 inputMovement;
    float inputJump;
    float inputForce;
    bool inputForceReleased;

    //Spectral Force
    public GameObject aimSphere;
    public GameObject attractedObject;

    public float maxDistance;
    public float forcePower;
    bool isAiming;
    RaycastHit hitAiming;
    float aimOffsetVelocityX;
    float aimOffsetVelocityY;
    float aimOffsetX;
    float aimOffsetY;
    public float aimOffsetSmooth;
    public float aimOffsetMaxDegree = 20f;
    RaycastHit activeHitAiming;
    bool nearTarget;
    float distanceNearTarget = 0.7f;

    public AnimationCurve forceJumpVelocityCurve;

    // DEBUG
    public Text debug1;
    public Text debug2;
    public Text debug3;

    // IK
    [Range (0,1f)]
    public float DistanceToGround;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        state = State.OnGround;
    }

    void Update()
    {

        Cursor.lockState = CursorLockMode.Locked;
        GetInput();
        HandleMovement();
        Aim();
        debug1.text = "Input  " + inputMovement;
        debug2.text = "IsUpWall " + isWallUp;
        debug3.text = "IsWall " + isWall;

    }

    void GetInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        inputMovement = new Vector3(horizontal, 0, vertical).normalized;

        inputForce = Input.GetAxisRaw("Fire1");
        if (Input.GetButtonUp("Fire1"))
            inputForceReleased = true;
        
        inputJump = Input.GetAxisRaw("Jump");
    }
    void HandleMovement()
    {
        Debug.Log(nearTarget);
        #region -------------    PHYSICAL CHECKS
        isGrounded = Physics.Raycast(groundCheck.position + transform.up, -transform.up, 1 + groundDistance, groundMask);
        isWall = Physics.Linecast(wallCheck.position-transform.forward*wallCheck.localPosition.z, wallCheck.position, groundMask);
        isWallUp = Physics.Linecast(wallUpCheck.position - transform.forward * wallUpCheck.localPosition.z, wallUpCheck.position, groundMask);

        Debug.DrawRay(groundCheck.position + transform.up, -transform.up*(1 + groundDistance), Color.red);
        Debug.DrawLine(wallCheck.position - transform.forward * wallCheck.localPosition.y, wallCheck.position, Color.red);
        Debug.DrawLine(wallUpCheck.position - transform.forward * wallUpCheck.localPosition.y, wallUpCheck.position, Color.red);
        #endregion
        #region -------------   ON GROUND
        if (state == State.OnGround)
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isHanging", false);
            animator.SetBool("isGliding", false);
            // -------------    RUN WALK
            if (inputMovement.magnitude >= 0.1f)
            {
                animator.SetBool("isWalking", true);

                float targetAngle = cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocityGrounded, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);


            }
            else
            {
                animator.SetBool("isWalking", false);
                inputMovement = new Vector3(0, 0, 0);
            }
            
        }
        #endregion
        #region -------------    ON WALL
        else if (state == State.OnWall)
        {
            animator.SetBool("isFalling", false);
            animator.SetBool("isHanging", true);

            //  aligner le perso a la surface de collision
            RaycastHit hitCollider;
            Vector3 originPosition = transform.position; //wallCheck.position - new Vector3(0,0,wallCheck.localPosition.z);
            Vector3 ForwardNeutral = transform.forward;
            ForwardNeutral.y = 0.0f;
            ForwardNeutral.Normalize();
            Debug.DrawLine(originPosition, originPosition + ForwardNeutral * 10, Color.yellow);
            if (Physics.Raycast(originPosition - ForwardNeutral * 1.2f, ForwardNeutral, out hitCollider, 10f))
            {
                Vector3 closestPoint = hitCollider.collider.ClosestPoint(originPosition);


                Debug.DrawLine(originPosition, closestPoint, Color.red);
                Vector3 direction = (closestPoint - transform.position);
                direction = Quaternion.AngleAxis(0, Vector3.up) * direction;

                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

                Debug.Log("rotated climb");
            }


            //rb.AddForce(transform.forward * 5f);
            if(inputMovement.magnitude < 0.1f)
            {
                rb.velocity = Vector3.SmoothDamp(rb.velocity, Vector3.zero, ref animSmoothVelocity, animSmoothTime);
            }
            bool isEdge = Physics.CheckSphere(wallUpCheck.position, groundDistance, groundMask);
            if (!isEdge)
            {
                animator.SetTrigger("climbEdge");
                animator.SetBool("isHanging", false);
            }
        }
        #endregion
        #region -------------    ON AIR
        if (state == State.OnAir)
        {
            animator.SetBool("isFalling", true);
            animator.SetBool("isHanging", false);
        }
        #endregion
        #region -------------    ON JUMP
        if (state == State.OnJump)
        {
            animator.SetBool("isFalling", false);
        }
        #endregion
        #region -------------    ON SPECTRAL FORCE
        if (state == State.OnAttractUse)
        {
            animator.SetBool("isFalling", false);
            SpectralForce();
        }
        #endregion
        #region  -------------    GRAVITY
        bool useGravity;
        switch (state)
        {
            case State.OnWall:      useGravity = false; break;
            case State.OnJump:      useGravity = false; break;
            case State.OnAttractUse:  useGravity = false; break;
            default: useGravity = true; break;
        }
        if (useGravity)
            rb.AddForce(Vector3.up * gravity);
        #endregion
        #region -------------    ROOT MOTION
        bool useRootMotion;
        switch (state)
        {
            case State.OnAttractUse:  useRootMotion = false; break;
            default:                useRootMotion = true; break;
        }
        if(useRootMotion) 
            animator.applyRootMotion = true;
        #endregion
        // ----------------------       ANIMATION
        // ----------   Run / walk
        animMovement = Vector3.SmoothDamp(animMovement, inputMovement, ref animSmoothVelocity, animSmoothTime);

        animator.SetFloat("Side", animMovement.x);
        animator.SetFloat("FrontBack", animMovement.z);

        #region -----------------------      STATES TRANSITION
        if (state == State.OnGround)
        {

            // ---------    TO ON WALL = aggrip, si on est devant le mur et qu'on va vers le mur
            if (isWall && inputMovement.z>0)
                Grab();
            // ---------    TO AIR = jump et fall
            if(inputJump >0)
                Jump();
            else if (!isGrounded)
                Fall();
            // ---------    TO ATTRACT USE
            if (inputForce != 0 && isAiming && inputForceReleased)
                Attract();
            // ---------    TO RESTING SUBSTATE
            if (inputMovement.magnitude <= 0.1f && inputForce == 0)
                timeSinceInput += Time.deltaTime;
            else
                timeSinceInput = 0;
            bool isResting;
            if (timeSinceInput > timeBeforeRest)
                isResting = true;
            else
                isResting = false;

            animator.SetBool("isResting", isResting);
        }
        else if(state == State.OnWall)
        {
            // -------- TO JUMP
            if (inputJump > 0)
                Jump();
            // -------- TO AIR
            if (!isWall)
                Fall();
            // -------- TO GROUND : descente et climb edge
            if (isGrounded)
                Ground();
            // -------- TO ATTRACT USE
            if (inputForce != 0 && isAiming && inputForceReleased)
                Attract();
        }
        else if(state == State.OnAir)
        {
            // ---- TO GROUND
            if (isGrounded)
                Ground();
            // ---- TO WALL
            else if (isWall && inputMovement.z > 0)
                Grab();
            // ---- TO ATTRACT
            if (inputForce != 0 && isAiming && inputForceReleased)
                Attract();
        }
        else if(state == State.OnJump)
        {
            // -------  TO AIR AUTO
            // -------- TO GROUND
            if (isGrounded)
                Ground();
            // -------- TO WALL
            else if (isWall && inputMovement.z > 0)
                Grab();
            // ----     TO ATTRACT
            if (inputForce != 0 && isAiming && inputForceReleased)
                Attract();
        }
        else if (state == State.OnAttractUse)
        {
            // -------- TO WALL
            if (nearTarget && isWall)
            {
                Debug.Log("go to wall");
                Grab();
            }
            // -------- TO AIR & GROUND
            if (inputForce == 0) {
                if (isGrounded)
                {
                    Fall();
                }
                else
                    Ground();
            }
        }
        #endregion


    }
    #region -------------   STATE TRANSITIONS FUNCTIONS
    private void Grab()
    {
        state = State.OnWall;
        if (isGrounded)
            StartCoroutine(GrabMove());
    }
    private IEnumerator GrabMove()
    {
        //TRANSITE SUR DU ON WALL, pousse le joueur vers le haut tant que is grounded est true
        rb.MovePosition(transform.position + transform.up * 0.1f);
        yield return new WaitWhile(() => isGrounded);
        rb.velocity = new Vector3(0, 0, 0);
    }
    private void Jump()
    {
        //TRANSITE SUR DU JUMP, lance l'anim
        animator.SetTrigger("Jump");
        state = State.OnJump;
        StartCoroutine(JumpEnd());
    }
    private IEnumerator JumpEnd()
    {
        yield return new WaitForSeconds(jumpAnimDuration);
        if (state == State.OnJump)
            state = State.OnAir;
    }
    private void Fall()
    {
        //TRANSITE SUR DU OnAir
        state = State.OnAir;
        animator.SetBool("isFalling", true);
    }
    private void Ground()
    {
        //TRANSITE SUR DU ground
        state = State.OnGround;

    }
    private void Attract()
    {
        activeHitAiming = hitAiming;
        inputForceReleased = false;
        state = State.OnAttractUse;
        rb.velocity = Vector3.zero; //reset la velocité quand on active la force;
    }
    #endregion
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundDistance);
        Gizmos.DrawSphere(wallCheck.position, groundDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(wallUpCheck.position, groundDistance);

    }
    void Aim()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, groundMask))
        {
            aimSphere.transform.position = hit.point;
            isAiming = true;
            hitAiming = hit;
        }
        else
        {
            isAiming = false;
        }
        if (state == State.OnAttractUse) {
            attractedObject.SetActive(true);
            attractedObject.transform.position = activeHitAiming.point;
        }
        else
            attractedObject.SetActive(false);
    }
    void SpectralForce()
    {
        // ROTATE TOWARD AIM
        Vector3 targetDirection = activeHitAiming.point - transform.position;
        float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocityGrounded, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);

        //  ----------  CROSS AIM ADJUSTEMENT
        aimOffsetX = Mathf.SmoothDamp(aimOffsetX, inputMovement.x, ref aimOffsetVelocityX, animSmoothTime);
        aimOffsetY = Mathf.SmoothDamp(aimOffsetY, inputMovement.z, ref aimOffsetVelocityY, animSmoothTime);

        Vector3 characterCenter = transform.position + rb.centerOfMass;
        Vector3 aimCenter = activeHitAiming.point;
        Vector3 aimDir = (aimCenter - characterCenter).normalized;

        float distanceToTarget = (activeHitAiming.point - (transform.position + rb.centerOfMass)).magnitude;

        aimDir = Quaternion.AngleAxis(aimOffsetMaxDegree*aimOffsetX, Vector3.up) * aimDir;
        aimDir = Quaternion.AngleAxis(aimOffsetMaxDegree * -aimOffsetY, Vector3.left) * aimDir;


        
        float forceOnChara = 0f;
        float forceOnCollider = 0f;

        float angleY = Vector3.Angle(aimDir, new Vector3(aimDir.x, 0, aimDir.z));
        if (angleY < 10) aimCenter = new Vector3(aimDir.x, 0, aimDir.z);

        Rigidbody colliderRb = activeHitAiming.transform.gameObject.GetComponent<Rigidbody>();


        nearTarget = distanceToTarget < distanceNearTarget;
        //  ----------  IMMOBILE OBJECT
        if (activeHitAiming.transform.gameObject.GetComponent<Rigidbody>() == null) {
            forceOnChara = forcePower * inputForce;
            forceOnCollider = 0;
            if (!nearTarget)
            {
                rb.AddForce(aimDir * forceOnChara);
            }
            Debug.DrawLine(transform.position, transform.position + aimDir * forceOnChara, Color.yellow);
            
        }
        //     -----------   MOBILE OBJECT
        else {

            float colliderMass = colliderRb.mass;
            forceOnCollider = forcePower * inputForce;
            forceOnChara = Mathf.Min(forcePower, colliderMass) * inputForce;
            colliderRb.AddForce(aimDir * -forceOnCollider);

            Debug.DrawLine(activeHitAiming.point, transform.position + rb.centerOfMass);
            Debug.DrawRay(transform.position + rb.centerOfMass, activeHitAiming.point - (transform.position + rb.centerOfMass));

            if (!nearTarget)
            {
                rb.AddForce(aimDir * forceOnChara);
            }
            Debug.DrawLine(transform.position, transform.position + aimDir * forceOnChara, Color.yellow);
        }

        // ANIMATION

        // TODO: CA MARCHE PAS, IL Y A UNE DOUILLE AVEC LE POID, verifier empiriquement quel est le facteur poid/force (produit en croix)
        if (isGrounded)
        {
            // ---------    Gliding
            if (Mathf.Abs(forceOnChara) > Mathf.Abs(forceOnCollider))
            {

                animator.SetBool("isGliding", true);
                animator.SetFloat("forceSign", inputForce * 0.5f + 0.5f);

                animator.SetBool("isArmForce", false);
            }
            // --------     Arm Push/pull
            else if (Mathf.Abs(forceOnChara) < Mathf.Abs(forceOnCollider))
            {
                animator.SetBool("isArmForce", true);
                animator.SetFloat("forceSign", inputForce*0.5f+0.5f);

                animator.SetBool("isGliding", false);
            }
        }
        else
        {
            animator.SetBool("isGliding", false);
            animator.SetBool("isArmForce", false);
            animator.SetFloat("forceSign", 0.5f);
        }

    }

    void OnAnimatorIK(int layerIndex)
    {
        /*
        if (animator)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);

            // Left Foot
            RaycastHit hit;
            Ray ray = new Ray(animator.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, groundMask))
            {
                Vector3 footPosition = hit.point;
                footPosition.y += DistanceToGround;
                animator.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                //animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));


            }
            // Right Foot
            ray = new Ray(animator.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, groundMask))
            {
                Vector3 footPosition = hit.point;
                footPosition.y += DistanceToGround;
                animator.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                //animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));

            }

        }
        */
        
    }
    
        
        

}
