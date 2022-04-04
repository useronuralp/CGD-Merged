using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
    public float bounceForce = 7.0f;
	void OnCollisionEnter(Collision collision)
	{
        Debug.Log("Collided with " + collision.transform.name);
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            if (collision.gameObject.tag == "Player")
            {
                Debug.Log("Collided");
                Vector3 hitDir = contact.normal;
                collision.gameObject.GetComponent<Ragdoll>().Bounce(hitDir, contact.point, bounceForce);
                return;
            }
        }
    }
}
