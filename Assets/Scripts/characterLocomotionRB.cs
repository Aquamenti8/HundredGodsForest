using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterLocomotionRB : MonoBehaviour
{
    //Third Person Basic Move
    Vector3 currentMovement;
    public Transform cam;
    Animator animator;
    Rigidbody rb;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    //GRAVITY
    Vector3 velocity;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;

    //Spectral Force
    public GameObject aimSphere;
    float spectralForce;
    [SerializeField] private LayerMask aimMask;
    public float maxDistance;
    public float forceMaxSpeed;
    bool isAiming;


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

        currentMovement = new Vector3(horizontal,0, vertical).normalized;


        spectralForce = Input.GetAxisRaw("Fire1");
    }
    void HandleMovement()
    {
        //MOVEMENT
        if(currentMovement.magnitude >= 0.1f)
        {
            animator.SetBool("isWalking", true);

            float targetAngle = Mathf.Atan2(currentMovement.x, currentMovement.z)*Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

        }
        else animator.SetBool("isWalking", false);

    }
    void Aim()
    {
        if (spectralForce == 0)
        {
            if (!rb.useGravity) rb.useGravity =true;
            var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, aimMask))
            {
                aimSphere.transform.position = hit.point;
                isAiming = true;
            }
            else
            {
                isAiming = false;
            }
        }
        else if(isAiming)
        {
            SpectralForce();
        }
    }
    void SpectralForce()
    {
        rb.useGravity = false;
        Vector3 characterCenter = transform.position + rb.centerOfMass;
        Vector3 aimCenter = aimSphere.transform.position;
        if (spectralForce == 1)  // PULL
        {
            Vector3 aimDir = (aimCenter - characterCenter).normalized * forceMaxSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + aimDir);

        }
        if (spectralForce == -1)  // PUSH
        {
            Vector3 aimDir = (characterCenter - aimCenter).normalized * forceMaxSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + aimDir);

        }
    }
        
        

}
