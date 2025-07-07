using Unity.VisualScripting;
using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject PlayerBullet;
    public float bulletSpawnDelay = 1f;//delay in seconds

    public float bulletSpeed = 1f;

    private bool isWaitingForShot = false;
    private float timer = 0f;
    public float shootingColldown = 2f;

    private Transform thisPlayerTransform;
    public Transform firePoint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisPlayerTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWaitingForShot)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartCoroutine(SpawnBulletCoroutine());
                timer = shootingColldown; //timer reset
            }
        }
    }


    IEnumerator SpawnBulletCoroutine()
    {
        isWaitingForShot = true;

        yield return new WaitForSeconds(bulletSpawnDelay); //delay
        SpawnBullet();

        isWaitingForShot = false;
    }
    void SpawnBullet()
    {
        float angleInDegrees = thisPlayerTransform.eulerAngles.z + 90f;
        float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));


        GameObject Bullet = Instantiate(PlayerBullet, firePoint.position, firePoint.rotation);
        // set owner of bullet
        Bullet.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rb = Bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;
    }
}
