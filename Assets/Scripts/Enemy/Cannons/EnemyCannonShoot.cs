
using UnityEngine;
using Mirror;

public class EnemyCannonShoot : Mirror.NetworkBehaviour, IShootReady
{
    [Header("FirePoint")]
    public Transform firePoint;
    protected Transform targetPlayer;
    protected Rigidbody2D rb;
    protected GameObject bulletsContainer;

    [Header("Reloading")]
    [SyncVar] protected bool waiting = true;
    public float minReloadDelay;
    public float maxReloadDelay;
    [SyncVar] protected float reloadDelay;
    [SyncVar] protected float reloadTimer = 0f;

    protected Enemy mainEnemy;

    [Server]
    public void ReadyToShoot()
    {
        waiting = false;
        // only for miniBoss or boss
        if(mainEnemy != null)
        {
            mainEnemy.OnArrival();
        }
    }

    [Server]
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
            mainEnemy = mainCannon;
        }

        reloadDelay = Random.Range(minReloadDelay, maxReloadDelay);
    }
}

