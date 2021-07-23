using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flyingCamera : MonoBehaviour
{
    public Camera cam;
    public worldGenerator world;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + (cam.transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal")), Time.deltaTime * 10f);
        transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y"), 0, 0));
        cam.transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X"), 0));

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1));
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {

                if (hit.transform.tag == "Terrain")
                {
                    Vector3 Pos = hit.transform.InverseTransformPoint(hit.point);
                    world.GetChunkFromVector3(hit.transform.position).PlaceTerrain(hit.point);
                }
                
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Terrain")
                {
                    Vector3 Pos = hit.transform.InverseTransformPoint(hit.point);
                    world.GetChunkFromVector3(hit.transform.position).RemoveTerrain(hit.point);
                }

            }
        }
    }
}
