using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunParticleRotate : MonoBehaviour
{
    public float rotationSpeed = 100;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, 1), rotationSpeed * Time.deltaTime);
    }
}
