using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Singleton
    void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("More than one GameManager in scene.");
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    private const string PLAYER_ID_PREFIX = "Player ";

    private static Dictionary<string, NetworkPlayer> players = new Dictionary<string, NetworkPlayer>();

    private static List<NetworkPlayer> playerList = new List<NetworkPlayer>();

    // Register the player
    public static void RegisterPlayer(string _netID, NetworkPlayer _player)
    {
        string _playerID = PLAYER_ID_PREFIX + _netID;

        // Check if there is already a player with that name
        if (players.ContainsKey(_playerID))
            return;

        players.Add(_playerID, _player);
        _player.transform.name = _playerID;

        // Add player to a list of NetworkPlayer
        playerList = GetAllPlayers();
    }

    // Unregister the player
    public static void UnRegisterPlayer(string _playerID)
    {
        players.Remove(_playerID);
    }

    // Get the player with that ID
    public static NetworkPlayer GetPlayer(string _playerID)
    {
        return players[_playerID];
    }

    // Return list of players
    public static List<NetworkPlayer> GetPlayerList()
    {
        return playerList;
    }

    // Get all the players
    public static List<NetworkPlayer> GetAllPlayers()
    {
        List<NetworkPlayer> _players = new List<NetworkPlayer>();

        for (int i = 0; i < players.Count; i++)
        {
            string _name = PLAYER_ID_PREFIX + i.ToString();

            if (!players.ContainsKey(_name))
            {
                Debug.LogWarning("Name: " + _name + " does not exist.");
                return null;
            }

            if (players[_name].GetComponent<NetworkPlayer>() != null)
            {
                _players.Add(players[_name]);
            }
        }
        return _players;
    }
}
