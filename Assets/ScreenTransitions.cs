using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
public class ScreenTransitions : MonoBehaviour
{
    public void LoadScene(int sceneIndex)
    {
        if(sceneIndex == 1)
        {
            PhotonNetwork.LoadLevel(sceneIndex); //This loading needs no synchronization so I am not using PhotonNetwork.LoadLevel() here.
            PhotonNetwork.JoinLobby();
        }
        else if (sceneIndex == 2)
        {
            PhotonNetwork.LoadLevel(sceneIndex);
        }
        else
        {
            if(PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(sceneIndex);
        }
    }
}
