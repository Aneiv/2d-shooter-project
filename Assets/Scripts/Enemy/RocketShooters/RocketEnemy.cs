using Mirror;
using System.Collections.Generic;
using UnityEngine;
public class RocketEnemy : EnemyShootBullet
{
    private Vector2 bulletPosition;
    //target player location
    protected List<Transform> targetPlayers = new List<Transform>();
    [SyncVar] protected Transform targetPlayer;

    override public void Start()
    {
        base.Start();

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
    }

    [ServerCallback]
    protected override void FixedUpdate() // dont aim at dead player
    {
        base.FixedUpdate();
        if(targetPlayer != null)
        {
            if (!targetPlayer.GetComponent<Player>().isAlive)
            {
                GetRandomPlayerTarget();
            }
        }
    }

    [Server]
    public void GetRandomPlayerTarget()
    {
        // filters dead players
        targetPlayers.RemoveAll(p => p == null || !p.GetComponent<Player>().isAlive);

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

    [Server]
    protected override void SpawnBullet()
    {
        GetRandomPlayerTarget();

        if (targetPlayer == null) return;

        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        //bullet start position
        bulletPosition.y = firePoint.position.y; //firePoint.bounds.size.y / 2;
        bulletPosition.x = firePoint.position.x;

        GameObject bullet = Instantiate(enemyBullet, bulletPosition, firePoint.rotation);
        bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        bullet.GetComponent<RocketBulletCollision>().Init(this.gameObject);

        var rocket = bullet.GetComponent<RocketBulletMovement>();
        rocket.target = targetPlayer; //give player position to bullet when spawned
        Mirror.NetworkServer.Spawn(bullet);
        RpcSetBulletVelocity(bullet, direction);
    }

    [ClientRpc]
    void RpcSetBulletVelocity(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}

