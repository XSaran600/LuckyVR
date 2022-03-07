using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class BettingObject : NetworkBehaviour
{
    // Set the cube to white when the server starts
    public override void OnStartServer()
    {
        base.OnStartServer();
        color = Color.white;
    }

    [SyncVar(hook = nameof(SetColor))]
    public Color32 color = Color.white;

    Material material;

    [SyncVar]
    public bool allPlayersPlacedBet = false;

    // SyncVar hook function to set the cube color
    void SetColor(Color32 _oldColor, Color32 _newColor)
    {
        if (material == null) material = GetComponentInChildren<Renderer>().material;
        material.color = _newColor;
    }

    // Destroy material
    void OnDestroy()
    {
        Destroy(material);
    }

    // Reset the color to white
    public void ResetColor()
    {
        // Only the server can reset the color
        if (!isServer)
            return;

        color = Color.white;
    }

    private void Update()
    {
        // Only the server can change the cube
        if (!isServer)
            return;

        List<NetworkPlayer> _players = GameManager.GetPlayerList();

        // Check if every player has placed a bet
        foreach (NetworkPlayer _player in _players)
        {
            if (_player.GetBetPlaced() == false)
            {
                allPlayersPlacedBet = false;
                return;
            }
        }

        allPlayersPlacedBet = true;
        RandomColor();
    }

    // Change the color to either green or red
    void RandomColor()
    {
        int _randomNum = Random.Range(0, 2);

        if (_randomNum == 0)
            color = Color.green;
        else
            color = Color.red;

    }

    // Return what color the cube is
    public int BetPlaced()
    {
        if (color == Color.green)
            return 0;
        else if (color == Color.red)
            return 1;
        else
            return 3;   // Error checking
    }

    // Return if all the players have placed a bet
    public bool DidAllPlayersPlacedBet()
    {
        return allPlayersPlacedBet;
    }


}
