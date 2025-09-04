
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

    protected Enemy mainEnemy;
    public void ReadyToShoot()
    {
        waiting = false;
        // only for miniBoss or boss
        if(mainEnemy != null)
        {
            mainEnemy.OnArrival();
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
            mainEnemy = mainCannon;
        }

        reloadDelay = Random.Range(minReloadDelay, maxReloadDelay);
    }
}

