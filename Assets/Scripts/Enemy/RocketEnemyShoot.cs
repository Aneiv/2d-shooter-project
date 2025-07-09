using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RocketEnemyShoot : MonoBehaviour
{

    public GameObject enemyBullet;
    private float bulletSpawnChance; //spawn chance
    public float bulletSpawnDelay = 1;//delay in seconds

    public float minSpawnChance = 0.5f;
    public float maxSpawnChance = 0.9f;

    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 2.0f;

    public float bulletSpeed = 1f;

    private bool waiting = false;
    private float checkTimer = 0f;
    public float checkInterval = 0.5f;    // checking chance delay

    private Transform thisEnemyTransform;
    private Vector2 bulletPosition;
    public Transform barrelTransform; //barrel transform

    //target player location
    private Transform targetPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisEnemyTransform = GetComponent<Transform>();
        //different spawn chance and delay for enemies to make diverse shooting style of same enemy type
        bulletSpawnChance = Random.Range(minSpawnChance, maxSpawnChance);
        bulletSpawnDelay = Random.Range(minSpawnDelay, maxSpawnDelay);

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }
    }

    // Update is called once per frame
    void Update()
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
        yield return new WaitForSeconds(bulletSpawnDelay); //delay
        SpawnBullet();
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
        Bullet.GetComponent<RocketBulletCollision>().Init(this.gameObject);

        var rocket = Bullet.GetComponent<RocketBulletMovement>();
        rocket.target = targetPlayer; //give player position to bullet when spawned
    }
}
