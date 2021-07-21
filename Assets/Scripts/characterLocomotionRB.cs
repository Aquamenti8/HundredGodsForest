using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class characterLocomotionRB : MonoBehaviour
{
    //Third Person Basic Move
    Vector3 inputMovement;
    public Transform cam;
    Animator animator;
    Rigidbody rb;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float gravity = -9.81f;

    //ANIMATION
    public float animSmoothTime = 0.1f;
    Vector3 animSmoothVelocity;
    Vector3 animMovement;
    bool isResting;
    float timeSinceInput;
    public float timeBeforeRest = 2f;

    bool isHanging;
    bool isGrounded;
    bool isWall;
    public Transform groundCheck;
    public Transform wallCheck;
    public float groundDistance = 0.1f;
    public LayerMask groundMask;

    //Spectral Force
    public GameObject aimSphere;
    float forceSign;
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
    }

    void Update()
    {

        Cursor.lockState = CursorLockMode.Locked;
        GetInput();
        HandleMovement();
        Aim();

    }

    void GetInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        inputMovement = new Vector3(horizontal, 0, vertical).normalized;


        forceSign = Input.GetAxisRaw("Fire1");
        
    }
    void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        isWall = Physics.CheckSphere(wallCheck.position, groundDistance, groundMask);
        //------------------------------    MOVEMENT
        if (isGrounded)
        {
            // -------------    RUN WALK
            if (inputMovement.magnitude >= 0.1f)
            {
                animator.SetBool("isWalking", true);

                float targetAngle = cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);


            }
            else
            {
                animator.SetBool("isWalking", false);
                inputMovement = new Vector3(0, 0, 0);
            }
        }
        // -------------    HANGING
        else if (isHanging)
        {
            //aligner le perso a la surface de collision
            Vector3 targetDirection = -hitAiming.normal;
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        // -------------    GRAVITY
        if (forceSign == 0)
        {
            rb.AddForce(Vector3.up * gravity);
            animator.applyRootMotion = true;
        }
        else
        {
            animator.applyRootMotion = false;
        }

        // ----------------------       ANIMATION
        // ----------   Run / walk
        animMovement = Vector3.SmoothDamp(animMovement, inputMovement, ref animSmoothVelocity, animSmoothTime);
        animator.SetFloat("Side", animMovement.x);
        animator.SetFloat("FrontBack", animMovement.z);

        // ---------    Resting
        if (inputMovement.magnitude <= 0.1f && forceSign == 0) timeSinceInput += Time.deltaTime;
        else timeSinceInput = 0;
        if (timeSinceInput > timeBeforeRest) isResting = true;
        else isResting = false;
        animator.SetBool("isResting", isResting);

        // ---------    Hanging

        if (!isGrounded && isWall)
        {
            isHanging = true;
        }
        else isHanging = false;
        animator.SetBool("isHanging", isHanging);



    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundDistance);
        Gizmos.DrawSphere(wallCheck.position, groundDistance);

    }
    void Aim()
    {
        if (forceSign == 0)
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

            //ANIM RESET
            animator.SetBool("isGliding", false);
            animator.SetBool("isArmForce", false);
            animator.SetFloat("forceSign", 0.5f);
        }
        else if (isAiming)
        {
            SpectralForce();

            // ROTATE TOWARD AIM
            Vector3 targetDirection = hitAiming.point - transform.position;
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        if (Input.GetButtonDown("Fire1") && isAiming) rb.velocity = Vector3.zero; //reset la velocité quand on active la force;
    }
    void SpectralForce()
    {
        //  ----------  CROSS AIM ADJUSTEMENT
        aimOffsetX = Mathf.SmoothDamp(aimOffsetX, inputMovement.x, ref aimOffsetVelocityX, animSmoothTime);
        aimOffsetY = Mathf.SmoothDamp(aimOffsetY, inputMovement.z, ref aimOffsetVelocityY, animSmoothTime);

        Vector3 characterCenter = transform.position + rb.centerOfMass;
        Vector3 aimCenter = hitAiming.point;
        Vector3 aimDir = (aimCenter - characterCenter).normalized;

        float distanceToTarget = (hitAiming.point - (transform.position + rb.centerOfMass)).magnitude;

        aimDir = Quaternion.AngleAxis(aimOffsetMaxDegree*aimOffsetX, Vector3.up) * aimDir;
        aimDir = Quaternion.AngleAxis(aimOffsetMaxDegree * -aimOffsetY, Vector3.left) * aimDir;


        float distanceThresh = 0.5f;
        float forceOnChara = 0f;
        float forceOnCollider = 0f;

        float angleY = Vector3.Angle(aimDir, new Vector3(aimDir.x, 0, aimDir.z));
        if (angleY < 10) aimCenter = new Vector3(aimDir.x, 0, aimDir.z);


        if (distanceToTarget < distanceThresh) Debug.Log("STOP");
        Rigidbody colliderRb = hitAiming.transform.gameObject.GetComponent<Rigidbody>();

       

        //  ----------  IMMOBILE OBJECT
        if (hitAiming.transform.gameObject.GetComponent<Rigidbody>() == null) {
            if (forceSign != 0)
            {
                forceOnChara = forcePower * forceSign;
                forceOnCollider = 0;
                if (distanceToTarget > distanceThresh)
                {
                    rb.AddForce(aimDir * forceOnChara);
                }
                Debug.DrawLine(transform.position, transform.position + aimDir * forceOnChara, Color.yellow);
            }
        }
        //     -----------   MOBILE OBJECT
        else {

            float colliderMass = colliderRb.mass;
            forceOnCollider = forcePower * forceSign;
            forceOnChara = Mathf.Min(forcePower, colliderMass) * forceSign;
            colliderRb.AddForce(aimDir * -forceOnCollider);

            Debug.DrawLine(hitAiming.point, transform.position + rb.centerOfMass);
            Debug.DrawRay(transform.position + rb.centerOfMass, hitAiming.point - (transform.position + rb.centerOfMass));

            if (distanceToTarget > distanceThresh)
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
                animator.SetFloat("forceSign", forceSign * 0.5f + 0.5f);

                animator.SetBool("isArmForce", false);
            }
            // --------     Arm Push/pull
            else if (Mathf.Abs(forceOnChara) < Mathf.Abs(forceOnCollider))
            {
                animator.SetBool("isArmForce", true);
                animator.SetFloat("forceSign", forceSign*0.5f+0.5f);

                animator.SetBool("isGliding", false);
            }
            debug1.text = "Force On Chara " + forceOnChara;
            debug2.text = "Force On Coll " + forceOnCollider;
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
    }
    
        
        

}
