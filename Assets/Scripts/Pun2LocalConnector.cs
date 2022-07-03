using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class Pun2LocalConnector
{

    static public void Setup()
    {
        Pun2LocalSettings p2locSet = new Pun2LocalSettings();

        if (File.Exists("pun2local.data"))
        {
            StreamReader stream = new StreamReader("pun2local.data");
            string dataJson = stream.ReadToEnd();
            stream.Close();

            p2locSet = JsonUtility.FromJson<Pun2LocalSettings>(dataJson);
        }
        else
        {
            StreamWriter stream = new StreamWriter("pun2local.data");
            stream.Write(JsonUtility.ToJson(p2locSet));
            stream.Close();
        }

        
        PhotonNetwork.PhotonServerSettings.AppSettings.Server = p2locSet.IP;
        PhotonNetwork.PhotonServerSettings.AppSettings.Port = p2locSet.PORT;
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = p2locSet.REGION;
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = p2locSet.USE_NAME_SERVER;

    }

}
