using System.Collections;
using UnityEngine;
public class LightRocketBulletCollision: RocketBulletCollision
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                //Debug.Log("LOG Bullet hit player");
                Destroy(gameObject);
                player.TakeDamage(damage);

            }
        }
        else if (collision.CompareTag("PlayerBullet"))
        {
            SturdyBullet bullet = GetComponent<SturdyBullet>();
            GameObject playerBullet = collision.gameObject;
            BulletCollisionDetection playerBulletCollision = playerBullet.GetComponent<BulletCollisionDetection>();
            if (bullet != null)
            {
                //Debug.Log($"LOG Player Bullet hit RocketBullet with damage: {shooterScript.BulletDamage}");
                Destroy(collision.gameObject);
                bullet.TakeDamage(playerBulletCollision.damage);
            }
        }
    }
}

