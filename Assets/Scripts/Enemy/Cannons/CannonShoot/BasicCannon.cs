using System.Collections;
using UnityEngine;
using Mirror;

public class BasicCannon : EnemyCannonShoot
{
    [Header("Bullet")]
    public GameObject enemyBullet;
    public float bulletSpeed = 3f;

    [Header("Shooting")]
    public int numberOfBulletInBurst = 3;
    public float bulletSpawnDelay = 0.3f;
    public float rotationSpeed = 200f;

    protected Vector2 targetPosition;
    protected Vector2 direction;

    public override void Start()
    {
        base.Start();
        reloadTimer = reloadDelay;
    }

    [Server]
    private void FixedUpdate()
    {
        if (!waiting)
        {
            reloadTimer -= Time.deltaTime;

            // aim
            targetPosition = targetPlayer.position;
            //destination
            Vector2 toTarget = targetPosition - rb.position;

            direction = toTarget.normalized;
            //rotation
            float rotateAmount = Vector3.Cross(direction, transform.up).z;

            rb.rotation -= rotateAmount * rotationSpeed * Time.deltaTime;

            // shoot
            if (reloadTimer <= 0f)
            {
                waiting = true;
                reloadTimer = Random.Range(minReloadDelay, maxReloadDelay);

                StartCoroutine(SpawnBulletCoroutine());
            }
        }
    }

    [Server]
    IEnumerator SpawnBulletCoroutine()
    {
        for (int i = 0; i < numberOfBulletInBurst; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(bulletSpawnDelay);
        }
        waiting = false;
    }

    [Server]
    void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        GameObject bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);

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


