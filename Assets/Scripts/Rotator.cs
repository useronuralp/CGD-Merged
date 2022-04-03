using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class Rotator : MonoBehaviourPunCallbacks
{
	public float speed = 3f;
    void Update()
    {
		transform.Rotate(0f, 0f, speed * Time.deltaTime / 0.01f, Space.Self);
	}
	[PunRPC]
	public void Sync(Vector3 position, Quaternion rotation)
	{
		transform.SetPositionAndRotation(position, rotation);
	}
	public override void OnPlayerEnteredRoom(Player other)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			photonView.RPC("Sync", RpcTarget.All, transform.position, transform.rotation);
		}
	}
}
