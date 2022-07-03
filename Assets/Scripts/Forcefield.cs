using UnityEngine;
using Photon.Pun;

public class Forcefield : MonoBehaviourPunCallbacks
{
    private GameObject  m_Owner; // Who creates this object during runtime.
    private Collider    m_OwnCollider; 
    private float       m_Lifetime = 5; // The lifetime that the forcefield will stay active for.
    private void Awake()
    {
        m_OwnCollider = GetComponent<Collider>();
        m_Owner = GameObject.Find(photonView.Owner.NickName);
        Collider[] colList = m_Owner.transform.GetComponentsInChildren<Collider>();
        foreach(Collider col in colList)
        {
            //Ignore collision with self and the owner.
            Physics.IgnoreCollision(col, m_OwnCollider, true);
        }
    }
    void Update()
    {
        if(photonView.IsMine)
        {
            if (m_Lifetime <= 0)
            {
                m_Owner.GetComponent<Game.PlayerManager>().DeactivateForcefield();
                PhotonNetwork.Destroy(gameObject);
            }
        }
        m_Lifetime -= Time.deltaTime;
        if (photonView.IsMine)
            transform.position = new Vector3(m_Owner.transform.position.x, m_Owner.transform.position.y + 0.5f, m_Owner.transform.position.z);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Bulldog") || collision.transform.CompareTag("Player") || collision.transform.CompareTag("Runner"))
        {
            Vector3 direction = transform.position - collision.transform.position;
            collision.transform.GetComponent<Game.Ragdoll>().Bounce(new Vector3(direction.x, direction.y + 3.0f, direction.z), new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), 2.0f);
        }
    }
}
