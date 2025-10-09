using Mirror;
using UnityEngine;

/// <summary>
/// Coin base class
/// </summary>
public class Coin : MonoBehaviour
{
    public int quantity = 5; //Money gain value
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!NetworkServer.active) return;
        if (collision.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.AddToCoins(quantity);
                var serverObject = GetComponent<ServerManagedObject>();
                serverObject.DestroyNetworkObject(this.gameObject);

                //--------
                //Old logic
                //Add collected money to pool
                //Transform terrain = gameObject.transform.parent.parent;
                //var endlessTerrain = terrain.GetComponent<EndlessTerrain>();
                //get correct chunk tile
                //TerrainChunk chunkTile = endlessTerrain.GetCurrentChunkByChild(gameObject);
                //update queue in that chunk tile
                //if (chunkTile != null)
                //{
                //    chunkTile.AddObjectToQueue(gameObject);
                //}
            }
        }
    }

}