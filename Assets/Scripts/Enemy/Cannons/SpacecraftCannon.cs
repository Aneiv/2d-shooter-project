using System.Collections;
using UnityEngine;

public class SpacecraftCannon : MonoBehaviour
{

    public Transform firePoint;
    private Transform targetPlayer;
    private Rigidbody2D rb;
    public GameObject enemyBullet;
    private GameObject bulletsContainer;

    private Vector2 targetPosition;
    private Vector2 direction;

    public float minReloadDelay = 2f;
    public float maxReloadDelay = 4f;

    public float rotationSpeed = 200f;
    public int numberOfBulletInBurst = 3;
    public float bulletSpawnDelay = 0.3f;
    public float bulletSpeed = 3f;

    private bool waiting = true;
    private float reloadTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bulletsContainer = GameObject.Find("BulletsContainer");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            targetPlayer = player.transform;
        }
        reloadTimer = Random.Range(minReloadDelay, maxReloadDelay);
    }

    public void ReadyToShoot()
    {
        waiting = false;
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
