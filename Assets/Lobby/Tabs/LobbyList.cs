using Mirror.Discovery;
using System.Collections.Generic;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    [SerializeField] private GameObject lobbyTilePrefab;
    [SerializeField] private Transform contentOfLobbyList;
    [SerializeField] private CustomNetworkDiscovery networkDiscovery;
    [SerializeField] private GameObject clientMenu;
    [SerializeField] private GameObject lobbyMenu;

    // dict of unique ip hosts
    private Dictionary<string, GameObject> activeLobbies = new Dictionary<string, GameObject>();
    void Start()
    {
        // listen for new servers
        networkDiscovery.OnServerFound.AddListener(OnServerFound);
        //networkDiscovery.StartDiscovery();

        //for(int i = 0; i < 3; i++)
        //{
        //    // new tile
        //    GameObject tileObj = Instantiate(lobbyTilePrefab, contentOfLobbyList);

        //    // set tile
        //    LobbyTile tile = tileObj.GetComponent<LobbyTile>();
        //    tile.Initialize(clientMenu, lobbyMenu);
        //    tile.SetText("Lobby" + i.ToString(), "8.8.8.8");
        //}
    }

    public void StartListening()
    {
        ClearLobbies();
        networkDiscovery.StartDiscovery();
    }

    private void OnServerFound(DiscoveryResponse info)
    {
        string ipAddress = info.ip;
        string serverName = info.serverName;

        if (activeLobbies.ContainsKey(ipAddress))
            return;

        Debug.Log("new server found: " + ipAddress);
        // new tile
        GameObject tileObj = Instantiate(lobbyTilePrefab, contentOfLobbyList);

        // set tile
        LobbyTile tile = tileObj.GetComponent<LobbyTile>();
        tile.Initialize(clientMenu, lobbyMenu);
        tile.SetText(serverName, ipAddress);

        activeLobbies[ipAddress] = tileObj;
    }

    private void ClearLobbies()
    {
        foreach (var tile in activeLobbies.Values)
        {
            if (tile != null)
                Destroy(tile);
        }

        activeLobbies.Clear();
    }
}
