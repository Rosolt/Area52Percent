using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkManagerOverride : NetworkManager
{
    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);
        Debug.Log(conn.hostId);
        if(conn.hostId != -1)
        {
            Debug.Log("Farmer");
            GameObject player = Instantiate(Resources.Load("FarmerPlayer"), transform.position, Quaternion.identity) as GameObject;
            NetworkServer.AddPlayerForConnection(conn, player, 0);
        }
        else
        {
            Debug.Log("UFO");
            GameObject player = Instantiate(Resources.Load("Spaceship"), transform.position, Quaternion.identity) as GameObject;
            NetworkServer.AddPlayerForConnection(conn, player, 0);
        }
    }
}
