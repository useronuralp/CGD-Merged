using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bounce : MonoBehaviour
{
	void OnCollisionEnter(Collision collision)
	{
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            if (collision.gameObject.tag == "Player")
            {
                Vector3 hitDir = contact.normal;
                collision.gameObject.GetComponent<Ragdoll>().Bounce(hitDir, contact.point);
                return;
            }
        }
    }
}
