using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class aimManager : MonoBehaviour
{
    [SerializeField] private LayerMask mask;
    public GameObject aimSphere;
    public float maxDistance;

    void Aim()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit, maxDistance, mask))
        {
            aimSphere.transform.position = hit.point;
            Debug.Log("Hit");

        }
    }
}
