using UnityEngine;
using static EndlessTerrain;

public class Coin : MonoBehaviour
{
    public int quantity = 5;
    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.AddToCoins(quantity);
                //Add collected money to pool
                Transform terrain = gameObject.transform.parent.parent;
                var endlessTerrain = terrain.GetComponent<EndlessTerrain>();
                //get correct chunk tile
                TerrainChunk chunkTile = endlessTerrain.GetCurrentChunkByChild(gameObject);
                //update queue in that chunk tile
                if (chunkTile != null)
                {
                    chunkTile.AddObjectToQueue(gameObject);
                }
            }
        }
    }
}
