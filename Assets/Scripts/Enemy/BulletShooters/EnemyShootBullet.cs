using Mirror;
using System.Collections;
using UnityEngine;

public class EnemyShootBullet : EnemyShoot
{
    [SyncVar] protected float bulletSpawnDelay;
    [Header("Spawn Delay")]
    public float checkInterval = 0.5f;    // checking chance delay
    public float minSpawnDelay;
    public float maxSpawnDelay;

    [Header("Spawn Chance")]
    public float minSpawnChance;
    public float maxSpawnChance;
    [SyncVar] private float bulletSpawnChance; //spawn chance - calculated in Start()

    [Header("Burst")]
    public int minNumberOfBulletInBurst = 2;
    public int maxNumberOfBulletInBurst = 5;
    [SyncVar] private int numberOfBulletInBurst;

    [Header("Bullet")]
    public GameObject enemyBullet;
    public float bulletSpeed;

    [Header("Fire Points")]
    public Transform firePoint;

    override public void Start()
    {
        base.Start();
        //different spawn chance and delay for enemies to make diverse shooting style of same enemy type
        bulletSpawnChance = Random.Range(minSpawnChance, maxSpawnChance);
        bulletSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
        numberOfBulletInBurst = Random.Range(minNumberOfBulletInBurst, maxNumberOfBulletInBurst);
    }
    //For Server use
    protected virtual void FixedUpdate()
    {
        if (!NetworkServer.active) return;
        if (!waiting)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = checkInterval; //timer reset

                if (Random.value < bulletSpawnChance)
                {
                    StartCoroutine(SpawnBulletCoroutine());
                }
            }
        }

    }

    [Server]
    IEnumerator SpawnBulletCoroutine()
    {
        waiting = true;
        for (int i = 0; i < numberOfBulletInBurst; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(bulletSpawnDelay); //delay
            
        }
        waiting = false;
    }

    [Server]
    virtual protected void SpawnBullet()
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

