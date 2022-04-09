using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Pendulum : MonoBehaviourPunCallbacks
{
	public float speed = 1.5f;
	public float limit = 75f; //Limit in degrees of the movement
	public bool randomStart = false; //If you want to modify the start position
	private float random = 0;
	private float time = 0;
	// Start is called before the first frame update
	void Awake()
    {
		if (PhotonNetwork.IsMasterClient)
        {
			if (randomStart)
				random = Random.Range(0f, 1f);
			time = Time.time;
        }
	}
	private void Start()
	{
		EventManager.Get().OnSyncObstacles += SyncRPC;
	}
	// Update is called once per frame
	void Update()
    {
		time += Time.deltaTime;
		float angle = limit * Mathf.Sin(time + random * speed);
		transform.localRotation = Quaternion.Euler(0, 0, angle);
	}
	[PunRPC]
	public void Sync(float time, float random, Quaternion rotation, Vector3 position)
    {
		this.time = time;
		this.random = random;
		transform.SetPositionAndRotation(position, rotation);
    }
	public override void OnPlayerEnteredRoom(Player other)
	{
		if(PhotonNetwork.IsMasterClient)
			photonView.RPC("Sync", RpcTarget.All, time, random, transform.rotation, transform.position);
	}
	public void SyncRPC()
	{
		if (PhotonNetwork.IsMasterClient)
			photonView.RPC("Sync", RpcTarget.All, time, random, transform.rotation, transform.position);
	}
}
