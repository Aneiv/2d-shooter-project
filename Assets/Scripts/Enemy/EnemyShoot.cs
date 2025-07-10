using System.Collections;
using UnityEngine;

public class EnemyShoot : MonoBehaviour
{

    public GameObject enemyBullet;
    private float bulletSpawnChance; //spawn chance - calculated in Start()
    public float bulletSpawnDelay = 1;//delay in seconds

    public float minSpawnChance;
    public float maxSpawnChance;

    public float minSpawnDelay;
    public float maxSpawnDelay;

    public float bulletSpeed;
    
    private bool waiting = false;
    private float checkTimer = 0f;
    public float checkInterval = 0.5f;    // checking chance delay


    private Transform thisEnemyTransform;
    private Vector2 bulletPosition;
    public Transform barrelTransform; //barrel transform
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisEnemyTransform = GetComponent<Transform>();
        //different spawn chance and delay for enemies to make diverse shooting style of same enemy type
        bulletSpawnChance = Random.Range(minSpawnChance, maxSpawnChance);
        bulletSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!waiting)
        {
            checkTimer -= Time.deltaTime;
            if (checkTimer <= 0f)
            {
                checkTimer = checkInterval; //timer reset

                if (Random.value < bulletSpawnChance)
                {
                    StartCoroutine(SpawnBulletCoroutine());
                }
            }
        }

    }

    IEnumerator SpawnBulletCoroutine()
    {
        waiting = true;
        SpawnBullet();        
        yield return new WaitForSeconds(bulletSpawnDelay); //delay
        waiting = false;
    }
    void SpawnBullet()
    {
        float angleInDegrees = thisEnemyTransform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        
        //bullet start position
        bulletPosition.y = barrelTransform.position.y; //thisEnemyRenderer.bounds.size.y / 2;
        bulletPosition.x = barrelTransform.position.x;
        
        GameObject Bullet = Instantiate(enemyBullet, bulletPosition, thisEnemyTransform.rotation);
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rb = Bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}
