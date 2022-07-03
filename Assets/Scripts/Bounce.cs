using UnityEngine;

/// <summary>
/// This class is used to send information to whoever collides with the game object that this script is attached to.
/// </summary>
public class Bounce : MonoBehaviour
{
    public float bounceForce = 7.0f;
	void OnCollisionEnter(Collision collision)
	{
        foreach (ContactPoint contact in collision.contacts)
        {
            if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Bulldog") || collision.gameObject.CompareTag("Runner"))
            {
                Vector3 hitDir = contact.normal;
                collision.gameObject.GetComponent<Game.Ragdoll>().Bounce(hitDir, contact.point, bounceForce);
                return;
            }
        }
    }
}
