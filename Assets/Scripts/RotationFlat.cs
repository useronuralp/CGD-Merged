using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class RotationFlat : MonoBehaviourPunCallbacks
{
    private void Awake()
    {
		EventManager.Get().OnSyncObstacles += SyncRPC;
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
	public void SyncRPC()
	{
		Debug.LogError("Synced RotationFlat");
		if (PhotonNetwork.IsMasterClient)
			photonView.RPC("Sync", RpcTarget.All, transform.position, transform.rotation);
	}
}