using System.Collections;
using UnityEngine;
public class TripleBarrelEnemy : EnemyShootBullet
{
    public Transform firePoint2;
    public Transform firePoint3;

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

            GameObject Bullet = Instantiate(enemyBullet, currentFirePoint.position, currentFirePoint.rotation);
            Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
            // set owner of bullet
            Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

            Rigidbody2D rb = Bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }
}
