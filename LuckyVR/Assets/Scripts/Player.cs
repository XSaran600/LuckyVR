using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour
{
    // enum to check the color the player has placed the bet on
    enum BetColor
    {
        Green,
        Red
    };

    BetColor betColor = BetColor.Green;

    // Stores the total amount and bet amount of chips
    int totalChips = 100;
    int bettingChips = 10;

    // UI gameobjects
    public GameObject waitForSecondPlayerUI;
    public GameObject greenRedUI;
    public GameObject betAmountUI;
    public GameObject waitForBetsUI;

    // Text objects
    public Text totalAmountText;
    public Text bettingAmountText;

    // The script to check the color of the betting object
    BettingObject bettingObject;

    // Our network player
    public NetworkPlayer networkPlayer;

    // Transform for where to spawn the chips
    public Transform chipPosition;
    public Transform redChipPosition;
    public Transform greenChipPosition;

    // The object pooler for the chips
    public ObjectPooler objectPooler;

    // Bool to check if both players have connected
    bool startOnce = true;

    void Start()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        totalAmountText.text = totalChips.ToString();

        // Check to make sure the betting object isn't null
        if (bettingObject == null)
            bettingObject = FindObjectOfType<BettingObject>();
    }

    // DeSpawn the chips
    void DeSpawnChips()
    {
        objectPooler.DeSpawnFromPool("Chips");
        objectPooler.DeSpawnFromPool("RedChips");
        objectPooler.DeSpawnFromPool("GreenChips");
    }

    // Organize the chips so they are 30 chips high for each pile
    Vector3 OrganizeChips(Vector3 _chipPos, int _i)
    {
        // Check to see if you are the sever or client because the client is rotated 180 degrees
        if (isServer)
            _chipPos.z = _chipPos.z + ((_i / 30) * 0.15f);
        else
            _chipPos.z = _chipPos.z - ((_i / 30) * 0.15f);

        return _chipPos;
    }

    // Spawn the chips
    void SpawnChips(int _amountOfChips, int _chipColor)
    {
        // Checks where to spawn the chips
        if (_chipColor == 0)  // Bank
        {
            for (int i = 0; i < _amountOfChips; i++)    // Loops through the amount of chips and spawns them
            {
                objectPooler.SpawnFromPool("Chips", OrganizeChips(chipPosition.position, i), chipPosition.rotation);    // Spawns using the object pooler
            }
        }
        else if (_chipColor == 1) // Green
        {
            for (int i = 0; i < _amountOfChips; i++)
            {
                objectPooler.SpawnFromPool("GreenChips", OrganizeChips(greenChipPosition.position, i), greenChipPosition.rotation);
            }
        }
        else if (_chipColor == 2) // Red
        {
            for (int i = 0; i < _amountOfChips; i++)
            {
                objectPooler.SpawnFromPool("RedChips", OrganizeChips(redChipPosition.position, i), redChipPosition.rotation);
            }
        }
    }

    // Makes the visual changes to match the current betting amount
    void ChangeBet()
    {
        if (betColor == BetColor.Green) // Green
        {
            DeSpawnChips(); // Despawn
            SpawnChips((totalChips - bettingChips) / 10, 0);    // Bank
            SpawnChips(bettingChips / 10, 1);   // Color
        }
        else if (betColor == BetColor.Red)    // Red
        {
            DeSpawnChips(); // Despawn
            SpawnChips((totalChips - bettingChips) / 10, 0);    // Bank
            SpawnChips(bettingChips / 10, 2);   // Color
        }
        else
        {
            Debug.LogError("ERROR");
        }
    }

    private void Update()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        CmdUpdate();
    }
    [Command]
    void CmdUpdate()
    {
        RpcUpdate();
    }
    [ClientRpc]
    void RpcUpdate()
    {
        // Check to make sure this only happens once
        if (startOnce)
        {
            // Once both players join allow the game to start
            if (GameManager.GetPlayerList().Count == 2)
            {
                waitForSecondPlayerUI.SetActive(false);
                greenRedUI.SetActive(true);

                SpawnChips(totalChips / 10, 0);

                startOnce = false;
            }
        }

        // Check to see if you have placed the bet
        if (waitForBetsUI.activeSelf)
        {
            // Check to make sure the betting object isn't null
            if (bettingObject == null)
                bettingObject = FindObjectOfType<BettingObject>();

            // Check to make sure all the players have placed a bet
            if (bettingObject.DidAllPlayersPlacedBet())
            {
                networkPlayer.SetBetPlaced(false);  // Unplace the bet

                // UI
                greenRedUI.SetActive(true);
                betAmountUI.SetActive(false);
                waitForBetsUI.SetActive(false);

                // Get the color of the betting object
                int _color = bettingObject.BetPlaced();

                // Add or subtract depending on if you win or lose
                if (betColor == BetColor.Green) // Green
                {
                    if (_color == 0)    // Green
                        totalChips = totalChips + bettingChips;
                    else if (_color == 1)   // Red
                        totalChips = totalChips - bettingChips;

                }
                else if (betColor == BetColor.Red)    // Red
                {
                    if (_color == 0)    // Green
                        totalChips = totalChips - bettingChips;
                    else if (_color == 1)   // Red
                        totalChips = totalChips + bettingChips;
                }
                else
                {
                    Debug.LogError("ERROR");
                }

                // Reset bet amount
                bettingChips = 10;

                // Add 100 chips if you lose all your chips
                if (totalChips <= 0)
                {
                    totalChips = 100;
                }

                // Text update
                bettingAmountText.text = bettingChips.ToString();
                totalAmountText.text = totalChips.ToString();

                DeSpawnChips(); // DeSpawn chips
                SpawnChips(totalChips / 10, 0); // Spawn chips
            }
        }
    }

    // Function called by button press to increase the bet
    public void IncreaseBet()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        CmdIncreaseBet();
    }
    [Command]
    void CmdIncreaseBet()
    {
        RpcIncreaseBet();
    }
    [ClientRpc]
    void RpcIncreaseBet()
    {
        // Check to see if the betting amount isn't greater than the amount you have
        if (bettingChips < totalChips)
        {
            bettingChips = bettingChips + 10;   // Increase bet amount
            bettingAmountText.text = bettingChips.ToString();   // Text change

            ChangeBet();    // Visual change
        }
    }

    // Function called by button press to decrease the bet
    public void DecreaseBet()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        CmdDecreaseBet();
    }
    [Command]
    void CmdDecreaseBet()
    {
        RpcDecreaseBet();
    }
    [ClientRpc]
    void RpcDecreaseBet()
    {
        // Check to see if the betting amount isn't greater than the minimum bet amount
        if (bettingChips > 10)
        {
            bettingChips = bettingChips - 10;   // Decrease bet amount
            bettingAmountText.text = bettingChips.ToString();   // Text change

            ChangeBet();    // Visual change
        }
    }

    // Function called by button press to confirm the bet
    public void ConfirmAmount()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        CmdConfirmAmount();
    }
    [Command]
    void CmdConfirmAmount()
    {
        RpcConfirmAmount();
    }
    [ClientRpc]
    void RpcConfirmAmount()
    {
        // Check to see if the betting amount is less than or equal to the amount you have
        if (bettingChips <= totalChips)
        {
            networkPlayer.SetBetPlaced(true);   // Place the bet

            // UI changes
            betAmountUI.SetActive(false);
            greenRedUI.SetActive(false);
            waitForBetsUI.SetActive(true);
        }
    }

    // Function called by button press to confirm the color of the bet
    public void Green()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        CmdGreen();
    }
    [Command]
    void CmdGreen()
    {
        RpcGreen();
    }
    [ClientRpc]
    void RpcGreen()
    {
        betColor = BetColor.Green;  // Set the bet color
        ConfirmColor(); // Confirm the bet
    }

    // Function called by button press to confirm the color of the bet
    public void Red()
    {
        // Make sure only the person with authority can make changes
        if (!hasAuthority)
            return;

        CmdRed();
    }
    [Command]
    void CmdRed()
    {
        RpcRed();
    }
    [ClientRpc]
    void RpcRed()
    {
        betColor = BetColor.Red;  // Set the bet color
        ConfirmColor(); // Confirm the bet
    }

    // Makes the UI and viusal changes
    void ConfirmColor()
    {
        // Check to make sure the betting object isn't null
        if (bettingObject == null)
            bettingObject = FindObjectOfType<BettingObject>();

        // Reset the color
        bettingObject.ResetColor();

        // UI change
        greenRedUI.SetActive(false);
        betAmountUI.SetActive(true);

        // Visual change
        ChangeBet();
    }
}
