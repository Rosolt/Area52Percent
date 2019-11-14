using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class NetworkManagerOverride : NetworkManager
{
    public Transform spawnPosition;
    public int curPlayer;
    /*public class NetworkMessage : MessageBase
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
                Debug.Log("spaceship");
                player = Instantiate(Resources.Load("Spaceship"), startPos.position, Quaternion.identity) as GameObject;
                NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            }
        }
        else
        {
            if(selectedClass == 0)
            {
                Debug.Log("spaceship");
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
            player = Instantiate(Resources.Load("Spaceship"), transform.position, Quaternion.identity) as GameObject;
        }
        else
        {
            test.chosenClass = 1;
            player = Instantiate(Resources.Load("FarmerPlayer"), transform.position, Quaternion.identity) as GameObject;
        }
        Debug.Log(player);
        Debug.Log("ClientConnect");
        ClientScene.RegisterPrefab(player);
        ClientScene.AddPlayer(conn, 0, test);
    }*/

    public override void OnClientConnect(NetworkConnection conn)
    {
        Debug.Log("here");
        //choose prefab based on host
        if (conn.hostId == -1)
        {
            curPlayer = 1;
        }
        else if (conn.hostId == 0)
        {
            curPlayer = 0;
        }
        //create message to set player
        IntegerMessage msg = new IntegerMessage(curPlayer);

        //call add player, pass msg
        ClientScene.AddPlayer(conn, 0, msg);
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
    {
        Debug.Log("hey");
        curPlayer = 0;
        Debug.Log(extraMessageReader.ReadMessage<IntegerMessage>().value);
        //read client msg
        if (extraMessageReader.ReadMessage<IntegerMessage>().value != null)
        {
            Debug.Log("in if");
            var stream = extraMessageReader.ReadMessage<IntegerMessage>();
            Debug.Log(stream);
            curPlayer = stream.value;
        }
        Debug.Log(curPlayer);
        Debug.Log("hello");
        //select prefab from spawn objects list
        var playerPrefab = spawnPrefabs[curPlayer];

        var player = Instantiate(playerPrefab, transform.position, Quaternion.identity) as GameObject;

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
}
