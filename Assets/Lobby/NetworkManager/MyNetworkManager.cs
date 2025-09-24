using Mirror;
using System.Collections;
using UnityEngine;

public class MyNetworkManager : Mirror.NetworkManager
{
    private int nextPlayerIndex = 0;
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
}
