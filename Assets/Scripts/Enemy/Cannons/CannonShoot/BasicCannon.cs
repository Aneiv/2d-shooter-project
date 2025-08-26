using System.Collections;
using UnityEngine;

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

    IEnumerator SpawnBulletCoroutine()
    {
        for (int i = 0; i < numberOfBulletInBurst; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(bulletSpawnDelay);
        }
        waiting = false;
    }

    void SpawnBullet()
    {
        float angleInDegrees = transform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        GameObject Bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);

        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rbBullet = Bullet.GetComponent<Rigidbody2D>();
        rbBullet.linearVelocity = direction.normalized * bulletSpeed;
    }
}

