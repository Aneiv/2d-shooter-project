using Mirror;
using UnityEngine;
public class TripleBarrelEnemy : EnemyShootBullet
{
    public Transform firePoint2;
    public Transform firePoint3;
    [Server]
    protected override void SpawnBullet()
    {
        Transform[] firePoints =
        {
            firePoint, firePoint2, firePoint3
        };

        foreach (Transform currentFirePoint in firePoints)
        {
            float angleInDegrees = currentFirePoint.eulerAngles.z;
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            GameObject bullet = Instantiate(enemyBullet, currentFirePoint.position, currentFirePoint.rotation);
            bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
            // set owner of bullet
            bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

            Mirror.NetworkServer.Spawn(bullet);
            RpcSetBulletVelocity(bullet, direction);
        }
    }
    [ClientRpc]
    void RpcSetBulletVelocity(GameObject bullet, Vector2 direction)
    {
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}
