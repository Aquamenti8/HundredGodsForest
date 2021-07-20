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


    //Spectral Force
    public GameObject aimSphere;
    float spectralForce;
    [SerializeField] private LayerMask aimMask;
    public float maxDistance;
    public float forcePower;
    bool isAiming;
    RaycastHit hitAiming;


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
            //if (!rb.useGravity) rb.useGravity =true;
            var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, aimMask))
            {
                aimSphere.transform.position = hit.point;
                isAiming = true;
                hitAiming = hit;
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
        //rb.useGravity = false;
        Vector3 characterCenter = transform.position + rb.centerOfMass;
        Vector3 aimCenter = hitAiming.point;
        // IMMOBILE OBJECT
        if (hitAiming.transform.gameObject.GetComponent<Rigidbody>() == null) {
            if (spectralForce == 1)  // PULL
            {
                Vector3 aimDir = (aimCenter - characterCenter).normalized * forcePower * Time.deltaTime;
                rb.MovePosition(transform.position + aimDir);

            }
            if (spectralForce == -1)  // PUSH
            {
                Vector3 aimDir = (characterCenter - aimCenter).normalized * forcePower * Time.deltaTime;
                rb.MovePosition(transform.position + aimDir);

            }
        }
        
        //MOBILE OBJECT
        else{
            Rigidbody colliderRb = hitAiming.transform.gameObject.GetComponent<Rigidbody>();
            Vector3 directionCol = (hitAiming.point - (transform.position + rb.centerOfMass)).normalized;
            Vector3 directionChara = (hitAiming.point - rb.centerOfMass- transform.position).normalized;
            float colliderMass = colliderRb.mass;

            Debug.Log("center of mass" + rb.centerOfMass);

            colliderRb.AddForce(directionCol * forcePower * spectralForce*-1);
            rb.MovePosition(transform.position + directionChara * spectralForce * Mathf.Min(colliderMass, forcePower)*Time.deltaTime);
            

        }
        
    }
        
        

}
