
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
    public void ReadyToShoot()
    {
        waiting = false;
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

        reloadDelay = Random.Range(minReloadDelay, maxReloadDelay);
    }
}

