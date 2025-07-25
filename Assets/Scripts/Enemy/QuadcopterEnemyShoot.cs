using System.Collections;
using UnityEngine;

public class QuadcopterEnemyShoot : MonoBehaviour, IEnemy
{

    public GameObject enemyBullet;
    private float bulletSpawnChance; //spawn chance - calculated in Start()
    public float bulletSpawnDelay = 0.5f;//delay in seconds

    public float minSpawnChance;
    public float maxSpawnChance;

    public int minNumberOfBulletInBurst = 2;
    public int maxNumberOfBulletInBurst = 5;

    public float bulletSpeed;
    
    private bool waiting = true;
    private float checkTimer = 0f;
    public float checkInterval = 3f;    // checking chance delay

    private int numberOfBulletInBurst;
    private GameObject bulletsContainer;
    private Transform thisEnemyTransform;
    private Vector2 bulletPosition;
    //public Transform barrelTransform; //barrel transform
    public Transform firePointLeft;
    public Transform firePointRight;
    public Transform firePointCenter;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bulletsContainer = GameObject.Find("BulletsContainer");
        thisEnemyTransform = GetComponent<Transform>();
        //different spawn chance and burst for enemies to make diverse shooting style of same enemy type
        bulletSpawnChance = Random.Range(minSpawnChance, maxSpawnChance);
        numberOfBulletInBurst = Random.Range(minNumberOfBulletInBurst, maxNumberOfBulletInBurst);
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
        for (int i = 0; i < numberOfBulletInBurst; i++)
        {
            SpawnBullet();        
            yield return new WaitForSeconds(bulletSpawnDelay); //delay
        }

        waiting = false;
    }
    void SpawnBullet()
    {
        Transform[] firePoints =
        {
            firePointLeft, firePointRight, firePointCenter
        };

        foreach(Transform firePoint in firePoints)
        {
            float angleInDegrees = firePoint.eulerAngles.z;
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));

            GameObject Bullet = Instantiate(enemyBullet, firePoint.position, firePoint.rotation);
            Bullet.transform.parent = bulletsContainer.transform; //make bullet child of 'BulletContainer'
            // set owner of bullet
            Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

            Rigidbody2D rb = Bullet.GetComponent<Rigidbody2D>();
            rb.linearVelocity = direction.normalized * bulletSpeed;
        }
    }

    public void OnArrival()
    {
        waiting = false;
    }
}
