using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Prototype.NetworkLobby;

public class NetworkLobbyHook : LobbyHook
{
    public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
    {
        LobbyPlayer lobby = lobbyPlayer.GetComponent<LobbyPlayer>();
        SetupLocalPlayer localPlayer = gamePlayer.GetComponent<SetupLocalPlayer>();

        if (lobby.playerName == "Player 1")
        {
            Debug.Log("UFO");
            localPlayer.pname = lobby.name;
            localPlayer.player = Instantiate(Resources.Load("Spaceship"), transform.position, Quaternion.identity) as GameObject;
            Debug.Log(lobby.localIcone);
        }
        else
        {
            localPlayer.pname = lobby.name;
            localPlayer.player = Instantiate(Resources.Load("FarmerPlayer"), transform.position, Quaternion.identity) as GameObject;
            Debug.Log("Farmer");
        }
    }
}
