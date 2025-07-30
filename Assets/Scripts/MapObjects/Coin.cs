using UnityEngine;

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
                Destroy(gameObject);
            }
        }
    }
}
