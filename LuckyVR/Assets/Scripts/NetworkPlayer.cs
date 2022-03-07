using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkPlayer : NetworkBehaviour
{
    public GameObject cameraAndUI;

    bool betPlaced = false;

    private void Start()
    {
        // Disable or enable camera depending if you are the local player
        if (isLocalPlayer)  // Server
        {
            cameraAndUI.SetActive(true);
        }
        else  // Client
        {
            cameraAndUI.SetActive(false);
        }
    }

    // On the start of the client register the player to the game manager
    public override void OnStartClient()
    {
        base.OnStartClient();

        uint _ID = GetComponent<NetworkIdentity>().netId - 2;
        string _netID = _ID.ToString();
        NetworkPlayer _player = GetComponent<NetworkPlayer>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    // On the stop of the client unregister the player to the game manager
    public override void OnStopClient()
    {
        GameManager.UnRegisterPlayer(transform.name);
    }

    // Set the players bet
    public void SetBetPlaced(bool _betPlaced)
    {
        if (!hasAuthority)
            return;

        CmdSetBetPlaced(_betPlaced);
    }
    [Command]
    void CmdSetBetPlaced(bool _betPlaced)
    {
        RpcSetBetPlaced(_betPlaced);
    }
    [ClientRpc]
    void RpcSetBetPlaced(bool _betPlaced)
    {
        betPlaced = _betPlaced;
    }

    // Return the players bet
    public bool GetBetPlaced()
    {
        return betPlaced;
    }

}
