using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterLocomotion : MonoBehaviour
{
    //Third Person Basic Move
    Vector3 currentMovement;
    public Transform cam;
    Animator animator;
    CharacterController controller;
    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    //Spectral Force
    public GameObject aimSphere;
    float spectralForce;
    [SerializeField] private LayerMask mask;
    public float maxDistance;
    public float forceMaxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
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
        //If there is some movement
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
            var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, mask))
            {
                aimSphere.transform.position = hit.point;
            }
        }
        else
        {
            SpectralForce();
        }
    }
    void SpectralForce()
    {
        if(spectralForce != 0)
        {
            if(spectralForce == 1)  // PULL
            {
                //move in aimSphereDir
                Vector3 aimDir = ( aimSphere.transform.position- transform.position).normalized;
                //controller.Move(aimDir*forceMaxSpeed*Time.deltaTime);
                //transform.Translate(aimDir * forceMaxSpeed * Time.deltaTime);
            }
        }
    }

}
