using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    public float bounceForce = 7.0f;
	void OnCollisionEnter(Collision collision)
	{
        foreach (ContactPoint contact in collision.contacts)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Vector3 hitDir = contact.normal;
                collision.gameObject.GetComponent<Ragdoll>().Bounce(hitDir, contact.point, bounceForce);
                return;
            }
        }
    }
}
