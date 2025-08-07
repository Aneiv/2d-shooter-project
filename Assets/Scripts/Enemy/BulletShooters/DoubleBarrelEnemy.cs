
using UnityEngine;

public class DoubleBarrelEnemy : EnemyShootBullet
{
    private bool shootLeft = true;
    public Transform firePoint2;

    public override void Start()
    {
        base.Start();
    }

    protected override void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Transform currentFirePoint = shootLeft ? firePoint : firePoint2;
        shootLeft = !shootLeft;

        GameObject Bullet = Instantiate(enemyBullet, currentFirePoint.position, currentFirePoint.rotation);
        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rb = Bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}

