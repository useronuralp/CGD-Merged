using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class ScreenTransitions : MonoBehaviour
{
    public void LoadScene(int sceneIndex)
    {
        if(PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel(sceneIndex);
    }
}
