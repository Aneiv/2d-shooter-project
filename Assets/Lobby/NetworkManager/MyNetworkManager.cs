using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyNetworkManager : Mirror.NetworkManager
{
    private int nextPlayerIndex = 0;
    private LostConnectionHandler lostConnectionHandler;

    public override void Start()
    {
        base.Start();
        lostConnectionHandler = GetComponent<LostConnectionHandler>();
        maxConnections = 2;
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        // Abort connection if game is running
        if (LanLobbyManager.Instance != null && LanLobbyManager.Instance.gameIsStarted)
        {
            Debug.Log("Aborted connection - game is running.");
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        // players spawn
        foreach (Mirror.NetworkConnectionToClient conn in Mirror.NetworkServer.connections.Values)
        {
            if (conn.identity == null)
            {
                //random position
                Vector3 spawnPosition = new Vector3(
                    UnityEngine.Random.Range(-2, 2),
                    UnityEngine.Random.Range(-4, -1),
                    0
                );
                //create player instance
                GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

                // set sprite
                if (player.TryGetComponent<PlayerSprite>(out var playerSprite)) {
                    int index = nextPlayerIndex % 2;
                    playerSprite.SetSpriteIndex(index);
                }

                if (!Mirror.NetworkClient.ready)
                {
                    Mirror.NetworkClient.Ready();
                }

                Mirror.NetworkServer.AddPlayerForConnection(conn, player);
                nextPlayerIndex++;
            }
        }
    }

    // lost connection handler
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        if (NetworkServer.active) return; // only client can lose connection

        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name != "MainGameScene") return;

        Debug.Log("Lost connection with host");

        SceneManager.LoadScene("MainMenuScene");

        if (lostConnectionHandler != null)
        {
            lostConnectionHandler.SetView();
        }

    }
}
