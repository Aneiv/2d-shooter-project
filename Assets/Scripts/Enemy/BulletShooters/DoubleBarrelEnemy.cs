
using Mirror;
using UnityEngine;

public class DoubleBarrelEnemy : EnemyShootBullet
{
    private bool shootLeft = true;
    public Transform firePoint2;

    public override void Start()
    {
        base.Start();
    }

    [Server]
    protected override void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Transform currentFirePoint = shootLeft ? firePoint : firePoint2;
        shootLeft = !shootLeft;

        GameObject bullet = Instantiate(enemyBullet, currentFirePoint.position, currentFirePoint.rotation);
        bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

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

