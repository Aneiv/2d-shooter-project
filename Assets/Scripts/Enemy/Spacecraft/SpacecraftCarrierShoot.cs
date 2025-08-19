using System.Collections;
using UnityEngine;

public class SpacecraftCarrierShoot : EnemyShoot
{
    private SpacecraftCarrierEnemy SpacecraftCarrierEnemy;
    public GameObject[] firePoints;
    public GameObject enemyBullet;
    private Transform targetPlayer;
    private Coroutine SpawnBulletsRef;

    float screenCenterYPos;

    public float bulletSpawnDelay = 0.5f;
    public float dectectionTime = 3f;
    public float bulletSpeed;

    public override void Start()
    {
        base.Start();
        SpacecraftCarrierEnemy = GetComponent<SpacecraftCarrierEnemy>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }

        Vector3 screenCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        screenCenterYPos = screenCenter.y;

        timer = dectectionTime;
    }

    private void FixedUpdate()
    {
        if (!waiting && SpacecraftCarrierEnemy.IsAlive()) {
            // detection of player
            if(targetPlayer.position.y >= screenCenterYPos && SpawnBulletsRef == null) 
            {
                timer -= Time.deltaTime;

                if(timer <= 0)
                {
                    SpawnBulletsRef = StartCoroutine(SpawnBulletsCoroutine());
                }
            }
            else
            {
                timer = dectectionTime;
            }
        }
    }

    private IEnumerator SpawnBulletsCoroutine()
    {
        waiting = true;
        foreach (GameObject firePointObj in firePoints) {
            Vector2 firePoint = firePointObj.transform.position;
            SpawnBullet(firePoint, 90f);
            SpawnBullet(firePoint, 270f);

            yield return new WaitForSeconds(bulletSpawnDelay);
        }
        waiting = false;
        SpawnBulletsRef = null;
    }

    private void SpawnBullet(Vector2 firePointPos, float angle)
    {
        float angleInDegrees = angle + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

        Quaternion rotation = Quaternion.Euler(0f, 0f, angleInDegrees);
        GameObject Bullet = Instantiate(enemyBullet, firePointPos, rotation);
        Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rb = Bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}
