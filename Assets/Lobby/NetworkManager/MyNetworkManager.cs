using Mirror;
using UnityEngine;

public class MyNetworkManager : Mirror.NetworkManager
{
    //after scene change on server
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        //players spawn
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
                Mirror.NetworkServer.AddPlayerForConnection(conn, player);
            }
        }
    }
}
