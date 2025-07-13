using Unity.VisualScripting;
using System.Collections;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject PlayerBulletBig;
    public GameObject PlayerBulletSmall;

    public float bulletSpawnDelay = 0.5f;//delay in seconds

    public float bulletSpeed = 1f;

    private bool isWaitingForShot = false;
    private float timer = 0f;
    public float shootingCooldown = 2f;

    private Transform thisPlayerTransform;
    public Transform firePoint_LBig;
    public Transform firePoint_RBig;

    public Transform firePoint_LSmall;
    public Transform firePoint_RSmall;

    private bool shootLeft = true;

    public int smallBulletCooldown = 1;
    private int bulletCounter = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisPlayerTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isWaitingForShot)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                StartCoroutine(SpawnBulletCoroutine());
                timer = shootingCooldown; //timer reset
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

        Transform firePointBig = shootLeft ? firePoint_LBig: firePoint_RBig;
        shootLeft = !shootLeft;

        GameObject BulletBig = Instantiate(PlayerBulletBig, firePointBig.position, firePointBig.rotation);
        bulletCounter++;
        
        // set owner of bullet
        BulletBig.GetComponent<BulletCollisionDetection>().Init(this.gameObject);

        Rigidbody2D rb_Big = BulletBig.GetComponent<Rigidbody2D>();
        rb_Big.linearVelocity = direction.normalized * bulletSpeed;

        if(bulletCounter== smallBulletCooldown)
        {
            bulletCounter = 0;
            GameObject BulletSmall_Left = Instantiate(PlayerBulletSmall, firePoint_LSmall.position, firePoint_LSmall.rotation);
            BulletSmall_Left.GetComponent<BulletCollisionDetection>().Init(this.gameObject);
            Rigidbody2D rb_Small_Left = BulletSmall_Left.GetComponent<Rigidbody2D>();
            rb_Small_Left.linearVelocity = direction.normalized * bulletSpeed;

            GameObject BulletSmall_Right = Instantiate(PlayerBulletSmall, firePoint_RSmall.position, firePoint_RSmall.rotation);
            BulletSmall_Right.GetComponent<BulletCollisionDetection>().Init(this.gameObject);
            Rigidbody2D rb_Small_Right = BulletSmall_Right.GetComponent<Rigidbody2D>();
            rb_Small_Right.linearVelocity = direction.normalized * bulletSpeed;
        }
    }
}
