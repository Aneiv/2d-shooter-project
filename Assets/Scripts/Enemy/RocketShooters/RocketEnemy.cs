using System.Collections;
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

    protected override void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        //bullet start position
        bulletPosition.y = firePoint.position.y; //firePoint.bounds.size.y / 2;
        bulletPosition.x = firePoint.position.x;

        GameObject Bullet = Instantiate(enemyBullet, bulletPosition, firePoint.rotation);
        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<RocketBulletCollision>().Init(this.gameObject);

        var rocket = Bullet.GetComponent<RocketBulletMovement>();
        rocket.target = targetPlayer; //give player position to bullet when spawned
    }
}

