using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkManagerOverride : NetworkManager
{

    public class NetworkMessage : MessageBase
    {
        public int chosenClass;
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        NetworkMessage message = extraMessageReader.ReadMessage<NetworkMessage>();
        int selectedClass = message.chosenClass;
        Debug.Log("selected class = " + selectedClass);
        GameObject player;
        Transform startPos = GetStartPosition();

        if(startPos != null)
        {
            if(selectedClass == 0)
            {
                player = Instantiate(Resources.Load("Spaceship"), startPos.position, Quaternion.identity) as GameObject;
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            }
        }
        else
        {
            if(selectedClass == 0)
            {
                player = Instantiate(Resources.Load("Spaceship"), transform.position, Quaternion.identity) as GameObject;
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            }
        }

    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        NetworkMessage test = new NetworkMessage();
        GameObject player;
        if (conn.hostId == -1)
        {
            test.chosenClass = 0;
            player = player = Instantiate(Resources.Load("Spaceship"), transform.position, Quaternion.identity) as GameObject;
        }
        else
        {
            test.chosenClass = 1;
            player = Instantiate(Resources.Load("FarmerPlayer"), transform.position, Quaternion.identity) as GameObject;
        }
        ClientScene.RegisterPrefab(player);
        ClientScene.AddPlayer(conn, 0, test);
    }

}
