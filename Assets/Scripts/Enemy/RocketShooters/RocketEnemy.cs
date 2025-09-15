using Mirror;
using UnityEngine;
public class RocketEnemy : EnemyShootBullet
{
    private Vector2 bulletPosition;
    //target player location
    private Transform targetPlayer;

    override public void Start()
    {
        base.Start();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }
    }
    [Server]
    protected override void SpawnBullet()
    {
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

