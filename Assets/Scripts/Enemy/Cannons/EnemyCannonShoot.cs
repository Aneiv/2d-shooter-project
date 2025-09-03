
using UnityEngine;

public class EnemyCannonShoot : MonoBehaviour, IShootReady
{
    [Header("FirePoint")]
    public Transform firePoint;
    protected Transform targetPlayer;
    protected Rigidbody2D rb;
    protected GameObject bulletsContainer;

    [Header("Reloading")]
    protected bool waiting = true;
    public float minReloadDelay;
    public float maxReloadDelay;
    protected float reloadDelay;
    protected float reloadTimer = 0f;

    protected Enemy enemyCannon;
    public void ReadyToShoot()
    {
        waiting = false;
        // only for miniBoss or boss
        if(enemyCannon != null)
        {
            enemyCannon.OnArrival();
        }
    }

    virtual public void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletsContainer = GameObject.Find("BulletsContainer");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }

        Enemy mainCannon = GetComponent<Enemy>();
        if (mainCannon != null) {
            enemyCannon = mainCannon;
        }

        reloadDelay = Random.Range(minReloadDelay, maxReloadDelay);
    }
}

