using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class PushRotators : MonoBehaviourPunCallbacks
{
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
