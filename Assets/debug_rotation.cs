using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class debug_rotation : MonoBehaviour
{
    float turnSmoothVelicityHanging;
    float turnSmoothTime = 0.1f;

    Vector3 closestPoint;

    // Update is called once per frame
    void Update()
    {
        //  aligner le perso a la surface de collision
        RaycastHit hitCollider;
        Debug.DrawLine(transform.position, transform.position + transform.forward* 10, Color.yellow);
        if (Physics.Raycast(transform.position - transform.forward * 1.2f, transform.forward, out hitCollider, 10f))
        {
            closestPoint = hitCollider.collider.ClosestPoint(transform.position);
            

            Debug.DrawLine(transform.position, closestPoint, Color.red);

            Vector3 direction = (closestPoint - transform.position);
            direction = Quaternion.AngleAxis(0, Vector3.up) * direction;

            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);

            
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(closestPoint, 1f);
    }
}
