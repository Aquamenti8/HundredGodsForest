using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class navMeshDestination : MonoBehaviour
{
    public NavMeshAgent TargetAgent;
    public bool moveOnClick = true;

    Animator animator;

    //PATROLLING
    public bool isPatrolling;
    private Vector3 targetPoint;
    private bool onDestination = true;
    private float timer;
    public Vector4 MovementRandomPointRange = new Vector4(25, -25, 25, -25);

    public LayerMask groundLayer;

    //Debug
    public Text debug1;
    public Text debug2;
    public Text debug3;

    //STAY UPWARD
    float turnSmoothVelocity;
    float turnSmoothTime = 0.1f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }


    void Update()
    {
        debug1.text = "Target point : " + targetPoint;
        debug2.text = "timer = " + timer;

        if (moveOnClick)
        {
            if (Input.GetMouseButtonDown(0))
            {
                MoveOnClick();
            }
        }
        if (isPatrolling) Patrolling();

        if(TargetAgent.velocity.magnitude > 0.5f)
        {
            animator.SetBool("isWalking", true);
        }
        else
        {
            animator.SetBool("isWalking", false);
        }

        //stayUpward TODO
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 3, groundLayer))
        {
            //aligner le perso a la surface de collision
            Vector3 targetDirection = -hit.normal;
            float targetAngle = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            //transform.rotation = Quaternion.AngleAxis(angle,);
        }
        
    }
    private void Patrolling()
    {
        if (onDestination)
        {
            timer -= Time.deltaTime;
            if (timer < 0f) ChooseNewDestination();
        }
        else
        {
            if (Vector3.Distance(transform.position, targetPoint) < 2f)
            {
                ReachDestination();
            }
        }
    }
    private void ChooseNewDestination()
    {
        targetPoint = new Vector3(Random.Range(MovementRandomPointRange.x, MovementRandomPointRange.y), 0f, Random.Range(MovementRandomPointRange.z, MovementRandomPointRange.w));

        RaycastHit hit;
        if(Physics.Raycast(targetPoint + Vector3.up * 1000f, Vector3.down, out hit, Mathf.Infinity, groundLayer))
        {
            NavMeshHit navMeshHit;
            if (NavMesh.SamplePosition(hit.point, out navMeshHit, 2, 1))
            {
                targetPoint = navMeshHit.position;
                TargetAgent.SetDestination(navMeshHit.position);
                onDestination = false;
            }
            else
            {
                Debug.Log("pas trouvé");
                TargetAgent.SetDestination(hit.point);
                
            }
        }

        
    }

    private void ReachDestination()
    {
        timer = Random.Range(1f, 5f);
        onDestination = true;
    }
    private void MoveOnClick()
    {
        if (TargetAgent)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit))
            {
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(hit.point, out navMeshHit, 1, 1))
                {
                    TargetAgent.SetDestination(navMeshHit.position);
                }
            }
        }
    }
}
