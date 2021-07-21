using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dynamicMass : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Calculate mass
        float volume = transform.localScale.x * transform.localScale.y * transform.localScale.z;

        // Set the mass
        GetComponent<Rigidbody>().mass = volume;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
