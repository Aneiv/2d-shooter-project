
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class EnemyCannonShoot : Mirror.NetworkBehaviour, IShootReady
{
    [Header("FirePoint")]
    public Transform firePoint;
    protected List<Transform> targetPlayers = new List<Transform>();
    [SyncVar] protected Transform targetPlayer;
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

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length > 0)
        {
            foreach (GameObject p in players)
            {
                if (p != null)
                    targetPlayers.Add(p.transform);
            }
        }
        GetRandomPlayerTarget();

        Enemy mainCannon = GetComponent<Enemy>();
        if (mainCannon != null) {
            mainEnemy = mainCannon;
        }

        reloadDelay = Random.Range(minReloadDelay, maxReloadDelay);
    }

    [Server]
    public void GetRandomPlayerTarget()
    {
        if (targetPlayers.Count > 0)
        {
            int index = Random.Range(0, targetPlayers.Count);
            targetPlayer = targetPlayers[index];
        }
        else
        {
            targetPlayer = null;
        }
    }
}

